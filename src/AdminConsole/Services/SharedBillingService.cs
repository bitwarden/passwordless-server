using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;

namespace Passwordless.AdminConsole.Services;

public class SharedBillingService<TDbContext> : ISharedBillingService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IPasswordlessManagementClient _passwordlessClient;
    private readonly ILogger<SharedBillingService<TDbContext>> _logger;
    private readonly StripeOptions _stripeOptions;

    public SharedBillingService(
        IDbContextFactory<TDbContext> dbContextFactory,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<SharedBillingService<TDbContext>> logger,
        IOptions<StripeOptions> stripeOptions)
    {
        _dbContextFactory = dbContextFactory;
        _passwordlessClient = passwordlessClient;
        _logger = logger;
        _stripeOptions = stripeOptions.Value;
    }

    /// <inheritdoc />
    public async Task UpdateUsageAsync()
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
            {
                g.Key.BillingSubscriptionItemId,
                Users = g.Sum(x => x.CurrentUserCount)
            })
            .ToListAsync();

        foreach (var item in items)
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var service = new UsageRecordService();
            try
            {
                await service.CreateAsync(
                    item.BillingSubscriptionItemId,
                    new UsageRecordCreateOptions
                    {
                        Quantity = item.Users,
                        Timestamp = DateTime.UtcNow,
                        Action = "set"
                    },
                    new RequestOptions
                    {
                        IdempotencyKey = idempotencyKey
                    }
                );
            }
            catch (StripeException e)
            {
                _logger.LogError("Usage report failed for item {BillingSubscriptionItemId}:", item.BillingSubscriptionItemId);
                _logger.LogError(e, "Idempotency key: {IdempotencyKey}.", idempotencyKey);
            }
        }
    }

    /// <inheritdoc />
    public async Task OnSubscriptionCreatedAsync(string customerId, string clientReferenceId, string subscriptionId)
    {
        // todo: Add extra error handling, if we already have a customerId on Org, throw.

        var orgId = int.Parse(clientReferenceId);

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

        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.GetAsync(subscriptionId);

        org.BillingCustomerId = customerId;
        org.BecamePaidAt = subscription.Created;
        org.BillingSubscriptionId = subscription.Id;

        // we only have one item per subscription
        SubscriptionItem lineItem = subscription.Items.Data.Single();
        var planName = _stripeOptions.Plans.Single(x => x.Value.PriceId == lineItem.Price.Id).Key;

        var features = _stripeOptions.Plans[planName].Features;

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

        var setFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod
        };

        // set the plans on each app
        foreach (var application in applications)
        {
            application.BillingPlan = planName;
            application.BillingSubscriptionItemId = lineItem.Id;
            application.BillingPriceId = lineItem.Price.Id;
            await _passwordlessClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }

        await db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateSubscriptionStatusAsync(Invoice? dataObject)
    {
        // todo: Handled paid or unpaid events
    }

    /// <inheritdoc />
    public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            throw new ArgumentNullException(nameof(subscriptionId));
        }
        var service = new SubscriptionService();
        var subscription = await service.CancelAsync(subscriptionId);

        return subscription.CancelAt.HasValue
               || subscription.CanceledAt.HasValue
               || subscription.Status == "canceled";
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
    public async Task OnApplicationDeletedAsync(
        string applicationId,
        string subscriptionItemId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var isSubscriptionItemInUse = await db.Applications.AnyAsync(x => x.BillingSubscriptionItemId == subscriptionItemId && x.Id != applicationId);
        if (!isSubscriptionItemInUse)
        {
            var subscriptionItemService = new SubscriptionItemService();
            await subscriptionItemService.DeleteAsync(subscriptionItemId);
        }
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

        var features = _stripeOptions.Plans[PlanConstants.Free].Features;
        organization.MaxAdmins = features.MaxAdmins;
        organization.MaxApplications = features.MaxApplications;

        foreach (var application in organization.Applications)
        {
            application.BillingPriceId = null;
            application.BillingSubscriptionItemId = null;
            application.BillingPlan = PlanConstants.Free;
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