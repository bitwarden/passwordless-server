using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
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
            try
            {
                await UpdateStripeAsync(organization.BillingSubscriptionItemId, organization.CurrentUserCount);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update usage for organization {orgId}: {error}", organization.Id, e.Message);
            }
            
        }
    }

    public async Task UpdateStripeAsync(string subscriptionItemId, int users)
    {
        // The idempotency key allows you to retry this usage record call if it fails.
        var idempotencyKey = Guid.NewGuid().ToString();

        DateTime timestamp = DateTime.UtcNow;
        var service = new UsageRecordService();
        try
        {
            UsageRecord? usageRecord = await service.CreateAsync(
                subscriptionItemId,
                new UsageRecordCreateOptions { Quantity = users, Timestamp = timestamp, Action = "set" },
                new RequestOptions { IdempotencyKey = idempotencyKey }
            );
        }
        catch (StripeException e)
        {
            Console.WriteLine($"Usage report failed for item {subscriptionItemId}:");
            Console.WriteLine($"{e} (idempotency key: {idempotencyKey})");
        }
    }

    public async Task ConvertFromFreeToPaidAsync(string customerId, string clientReferenceId, string subscriptionId)
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
        org.BillingSubscriptionId = subscription.Id;

        // Increase the limits
        org.MaxAdmins = 1000;
        org.MaxApplications = 1000;

        // we only have one item per subscription
        SubscriptionItem lineItem = subscription.Items.Data.Single();
        var planName = lineItem.Plan.Product.Name;

        if (_stripeOptions.Plans.All(x => x.Value.PriceId != lineItem.Price.Id))
        {
            throw new InvalidOperationException("Received a subscription for a product that is not configured.");
        }
        
        org.BillingPlan = planName;
        org.BillingSubscriptionItemId = lineItem.Id;

        var applications = await db.Applications
            .Where(a => a.OrganizationId == orgId)
            .Select(x => x.Id)
            .ToListAsync();
        
        var features = _plansOptions[planName];
        var setFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod
        };
        
        // set the plans on each app
        foreach (var applicationId in applications)
        {
            await _passwordlessClient.SetFeaturesAsync(applicationId, setFeaturesRequest);
        }

        await db.SaveChangesAsync();
    }

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

    public async Task<string?> GetCustomerIdAsync(int organizationId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var customerId = await db.Organizations
            .Where(o => o.Id == organizationId)
            .Select(o => o.BillingCustomerId)
            .FirstOrDefaultAsync();
        return customerId;
    }
}