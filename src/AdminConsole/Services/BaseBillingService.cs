using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services;

public record UsageItem(string BillingSubscriptionItemId, int Users);

public class BaseBillingService<TDbContext> where TDbContext : ConsoleDbContext
{
    protected readonly IDbContextFactory<TDbContext> _dbContextFactory;

    protected readonly IPasswordlessManagementClient _passwordlessClient;
    protected readonly ILogger<BaseBillingService<TDbContext>> _logger;
    protected readonly BillingOptions _billingOptions;
    protected readonly IDataService _dataService;
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly IActionContextAccessor _actionContextAccessor;

    public BaseBillingService(
        IDbContextFactory<TDbContext> dbContextFactory,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<BaseBillingService<TDbContext>> logger,
        IOptions<BillingOptions> billingOptions,
        IActionContextAccessor actionContextAccessor,
        IUrlHelperFactory urlHelperFactory

        )
    {
        _dbContextFactory = dbContextFactory;
        _dataService = dataService;
        _passwordlessClient = passwordlessClient;
        _logger = logger;
        _billingOptions = billingOptions.Value;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
    }

    protected async Task SetPlanOnApp(string app, string selectedPlan, string subscriptionItemId, string priceId)
    {
        var plan = _billingOptions.Plans[selectedPlan];
        await this.UpdateApplicationAsync(app, selectedPlan, subscriptionItemId, priceId);

        var updateFeaturesRequest = new ManageFeaturesRequest
        {
            EventLoggingIsEnabled = plan.Features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = plan.Features.EventLoggingRetentionPeriod,
            MagicLinkEmailMonthlyQuota = plan.Features.MagicLinkEmailMonthlyQuota,
            MaxUsers = plan.Features.MaxUsers,
            AllowAttestation = plan.Features.AllowAttestation
        };

        await _passwordlessClient.SetFeaturesAsync(app, updateFeaturesRequest);
    }

    protected async Task UpgradeToPaidOrganization(string customerId, string planName, int orgId, string subscriptionId, DateTime subscriptionCreatedAt, string subscriptionItemId, string subscriptionItemPriceId)
    {
        var features = _billingOptions.Plans[planName].Features;

        // SetCustomerId on the Org
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var org = await db.Organizations.FirstOrDefaultAsync(x => x.Id == orgId);

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


        var applications = await db.Applications
            .Where(a => a.OrganizationId == orgId)
            .ToListAsync();

        var setFeaturesRequest = new ManageFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod,
            MagicLinkEmailMonthlyQuota = features.MagicLinkEmailMonthlyQuota,
            MaxUsers = features.MaxUsers,
            AllowAttestation = features.AllowAttestation
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

        var features = _billingOptions.Plans[_billingOptions.Store.Free].Features;
        organization.MaxAdmins = features.MaxAdmins;
        organization.MaxApplications = features.MaxApplications;

        foreach (var application in organization.Applications)
        {
            application.BillingPriceId = null;
            application.BillingSubscriptionItemId = null;
            application.BillingPlan = _billingOptions.Store.Free;
        }

        await db.SaveChangesAsync();

        var setFeaturesRequest = new ManageFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod,
            MaxUsers = features.MaxUsers,
            AllowAttestation = features.AllowAttestation
        };
        foreach (var application in organization.Applications)
        {
            await _passwordlessClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }
    }


}

public record PricingCardModel(
    string Name,
    BillingPlanOptions Plan)
{
    /// <summary>
    /// We want to display the price in US dollars.
    /// </summary>
    private static readonly CultureInfo PriceFormat = new("en-US");

    /// <summary>
    /// Indicates if the plan is the active plan for the organization.
    /// </summary>
    public bool IsActive { get; set; }
}

public record PaymentMethodModel(string Brand, string Number, DateTime ExpirationDate)
{
    public string CardIcon
    {
        get
        {
            var path = new StringBuilder("Shared/Icons/PaymentMethods/");
            switch (Brand)
            {
                case "amex":
                    path.Append("Amex");
                    break;
                case "diners":
                    path.Append("Diners");
                    break;
                case "discover":
                    path.Append("Discover");
                    break;
                case "jcb":
                    path.Append("Jcb");
                    break;
                case "mastercard":
                    path.Append("MasterCard");
                    break;
                case "unionpay":
                    path.Append("UnionPay");
                    break;
                case "visa":
                    path.Append("Visa");
                    break;
                default:
                    path.Append("UnknownCard");
                    break;
            }
            return path.ToString();
        }
    }
}