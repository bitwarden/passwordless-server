using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
using Passwordless.AdminConsole.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;

namespace Passwordless.AdminConsole.Services;

public class SharedBillingService<TDbContext> : ISharedBillingService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IPasswordlessManagementClient _passwordlessClient;
    private readonly PlansOptions _plansOptions;
    private readonly ILogger<SharedBillingService<TDbContext>> _logger;
    private readonly StripeOptions _stripeOptions;

    public SharedBillingService(
        IDbContextFactory<TDbContext> dbContextFactory,
        IPasswordlessManagementClient passwordlessClient,
        IOptions<PlansOptions> plansOptions,
        ILogger<SharedBillingService<TDbContext>> logger,
        IOptions<StripeOptions> stripeOptions)
    {
        _dbContextFactory = dbContextFactory;
        _passwordlessClient = passwordlessClient;
        _plansOptions = plansOptions.Value;
        _logger = logger;
        _stripeOptions = stripeOptions.Value;
    }

    /// <inheritdoc />
    public async Task UpdateUsageAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var organizations = await db.Organizations
            .Where(x => x.BillingSubscriptionItemId != null)
            .Select(o => new
            {
                o.Id,
                o.BillingSubscriptionItemId,
                CurrentUserCount = o.Applications.Sum(a => a.CurrentUserCount)
            })
            .ToListAsync();

        foreach (var organization in organizations)
        {
            var idempotencyKey = Guid.NewGuid().ToString();
            var service = new UsageRecordService();
            try
            {
                await service.CreateAsync(
                    organization.BillingSubscriptionItemId,
                    new UsageRecordCreateOptions
                    {
                        Quantity = organization.CurrentUserCount,
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
                _logger.LogError("Usage report failed for item {BillingSubscriptionItemId}:", organization.BillingSubscriptionItemId);
                _logger.LogError(e, "Idempotency key: {IdempotencyKey}.", idempotencyKey);
            }
        }
    }

    /// <inheritdoc />
    public async Task OnPaidSubscriptionChangedAsync(string customerId, string clientReferenceId, string subscriptionId)
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
        org.BillingCustomerId = customerId;

        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.GetAsync(subscriptionId);

        // If subscription is not set, it means we were using a free plan.
        if (org.BillingSubscriptionId == null)
        {
            // Customer started paying for the first time
            org.BecamePaidAt = subscription.Created;
        }

        // set the subscriptionId on the Org
        var existingSubscriptionId = org.BillingSubscriptionId;
        org.BillingSubscriptionId = subscription.Id;

        // we only have one item per subscription
        SubscriptionItem lineItem = subscription.Items.Data.Single();
        var planName = _stripeOptions.Plans.Single(x => x.Value.PriceId == lineItem.Price.Id).Key;

        var features = _plansOptions[planName];

        // Increase the limits
        org.MaxAdmins = features.MaxAdmins;
        org.MaxApplications = features.MaxApplications;

        if (planName == null)
        {
            throw new InvalidOperationException("Received a subscription for a product that is not configured.");
        }

        org.BillingPlan = planName;
        org.BillingSubscriptionItemId = lineItem.Id;

        var applications = await db.Applications
            .Where(a => a.OrganizationId == orgId)
            .Select(x => x.Id)
            .ToListAsync();

        var setFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod
        };

        // If we had a subscription before, cancel it now as we've successfully subscribed to the new plan.
        if (existingSubscriptionId != null && existingSubscriptionId != subscription.Id)
        {
            // Issue prorated invoice.
            var cancelOptions = new SubscriptionCancelOptions
            {
                InvoiceNow = true,
                Prorate = true
            };
            await subscriptionService.CancelAsync(existingSubscriptionId, cancelOptions);
        }

        // set the plans on each app
        foreach (var applicationId in applications)
        {
            await _passwordlessClient.SetFeaturesAsync(applicationId, setFeaturesRequest);
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


    /// <inheritdoc />
    public async Task OnSubscriptionDeletedAsync(string subscriptionId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var organization = await db.Organizations.Include(x => x.Applications).SingleOrDefaultAsync(x => x.BillingSubscriptionId == subscriptionId);
        if (organization == null) return;
        organization.BillingPlan = PlanConstants.Free;
        organization.BillingSubscriptionId = null;
        organization.BillingSubscriptionItemId = null;
        organization.BecamePaidAt = null;

        var features = _plansOptions[PlanConstants.Free];
        organization.MaxAdmins = features.MaxAdmins;
        organization.MaxApplications = features.MaxApplications;
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