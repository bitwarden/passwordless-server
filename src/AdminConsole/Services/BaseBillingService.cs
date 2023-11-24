using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Services;

public record UsageItem(string BillingSubscriptionItemId, int Users);

public class BaseBillingService<TDbContext> where TDbContext : ConsoleDbContext
{
    protected readonly IDbContextFactory<TDbContext> _dbContextFactory;

    protected readonly IPasswordlessManagementClient _passwordlessClient;
    protected readonly ILogger<SharedBillingService<TDbContext>> _logger;
    protected readonly StripeOptions _stripeOptions;
    protected readonly IDataService _dataService;

    public BaseBillingService(
        IDbContextFactory<TDbContext> dbContextFactory,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<SharedBillingService<TDbContext>> logger,
        IOptions<StripeOptions> stripeOptions)
    {
        _dbContextFactory = dbContextFactory;
        _dataService = dataService;
        _passwordlessClient = passwordlessClient;
        _logger = logger;
        _stripeOptions = stripeOptions.Value;
    }
    
    
    
    protected async Task SetFeatures(string customerId, string planName, int orgId, string subscriptionId, DateTime subscriptionCreatedAt, string subscriptionItemId, string subscriptionItemPriceId)
    {
        var features = _stripeOptions.Plans[planName].Features;


        // SetCustomerId on the Org
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var org = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync<Organization>(db.Organizations, x => x.Id == orgId);

        if (org == null)
        {
            throw new InvalidOperationException("Org not found");
        }

        if (org.HasSubscription)
        {
            return;
        }


        org.BillingCustomerId = customerId;
        org.BecamePaidAt = subscriptionCreatedAt;
        org.BillingSubscriptionId = subscriptionId;


        // Increase the limits
        org.MaxAdmins = features.MaxAdmins;
        org.MaxApplications = features.MaxApplications;

        if (planName == null)
        {
            throw new InvalidOperationException("Received a subscription for a product that is not configured.");
        }


        var applications = await Queryable
            .Where<Application>(db.Applications, a => a.OrganizationId == orgId)
            .ToListAsync();

        var setFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod
        };

        // set the plans on each app
        foreach (var application in applications)
        {
            application.BillingPlan = planName;
            application.BillingSubscriptionItemId = subscriptionItemId;
            application.BillingPriceId = subscriptionItemPriceId;
            await _passwordlessClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }

        await db.SaveChangesAsync();
    }

    protected async Task<List<UsageItem>> GetUsageItems()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var items = await db.Applications
            .Where(a => a.BillingSubscriptionItemId != null)
            .GroupBy(a => new
            {
                a.OrganizationId,
                a.BillingSubscriptionItemId
            })
            .Select(g => new
                UsageItem(g.Key.BillingSubscriptionItemId, g.Sum(x => x.CurrentUserCount)))
            .ToListAsync();
        return items;
    }

    /// <inheritdoc />
    public async Task<string?> GetCustomerIdAsync(int organizationId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var customerId = await db.Organizations
            .Where(o => o.Id == organizationId)
            .Select(o => o.BillingCustomerId)
            .FirstOrDefaultAsync();
        return customerId;
    }

    public async Task UpdateApplicationAsync(string applicationId, string plan, string subscriptionItemId, string priceId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Applications
            .Where(x => x.Id == applicationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.BillingPlan, plan)
                .SetProperty(p => p.BillingSubscriptionItemId, subscriptionItemId)
                .SetProperty(p => p.BillingPriceId, priceId));
    }

    /// <inheritdoc />
    public async Task OnSubscriptionDeletedAsync(string subscriptionId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organization = await db.Organizations
            .Include(x => x.Applications)
            .SingleOrDefaultAsync(x => x.BillingSubscriptionId == subscriptionId);
        if (organization == null) return;
        organization.BillingSubscriptionId = null;
        organization.BecamePaidAt = null;

        var features = _stripeOptions.Plans[_stripeOptions.Store.Free].Features;
        organization.MaxAdmins = features.MaxAdmins;
        organization.MaxApplications = features.MaxApplications;

        foreach (var application in organization.Applications)
        {
            application.BillingPriceId = null;
            application.BillingSubscriptionItemId = null;
            application.BillingPlan = _stripeOptions.Store.Free;
        }

        await db.SaveChangesAsync();

        var setFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod
        };
        foreach (var application in organization.Applications)
        {
            await _passwordlessClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }
    }
}