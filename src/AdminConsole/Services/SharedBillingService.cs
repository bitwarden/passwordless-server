using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing;
using Passwordless.AdminConsole.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;
using Application = Passwordless.AdminConsole.Models.Application;

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
        IOptionsSnapshot<PlansOptions> plansOptions,
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
        List<Application> apps = await db.Applications.Where(x => x.BillingPriceId != null)
            .ToListAsync();

        // update usage in stripe
        foreach (Application app in apps)
        {
            try
            {
                if (app.BillingSubscriptionItemId == null)
                {
                    await AddBillingSubscriptionItemId(app);
                }

                var users = app.CurrentUserCount;
                await UpdateStripeAsync(app.BillingSubscriptionItemId, users);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to update usage for app {appId}: {error}", app.Id, e.Message);
            }
        }
    }

    private async Task AddBillingSubscriptionItemId(Application app)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var org = await db.Organizations.FirstOrDefaultAsync(x => x.Id == app.OrganizationId);
        Subscription subscription = await GetSubscription(org.BillingSubscriptionId);

        // Increase the limits
        org.MaxAdmins = 1000;
        org.MaxApplications = 1000;

        // get the lineItem from the subscription
        SubscriptionItem? lineItem = subscription.Items.Data.FirstOrDefault(i => i.Price.Id == app.BillingPriceId);
        app.BillingSubscriptionItemId = lineItem.Id;
        await db.SaveChangesAsync();
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

        var priceId = _stripeOptions.UsersProPriceId;
        var planName = _stripeOptions.UsersProPlanName;
        var orgId = int.Parse(clientReferenceId);

        // SetCustomerId on the Org
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var org = await db.Organizations.FirstOrDefaultAsync(x => x.Id == orgId);
        if (org == null)
        {
            throw new InvalidOperationException("Org not found");
        }
        org.BillingCustomerId = customerId;

        // Create a new Subscription
        Subscription subscription = await GetSubscription(subscriptionId);

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

        // get the lineItem from the subscription
        SubscriptionItem? lineItem = subscription.Items.Data.FirstOrDefault(i => i.Price.Id == priceId);

        // set the new plans on each app
        List<Application> apps = await db.Applications.Where(a => a.OrganizationId == orgId).ToListAsync();
        var features = _plansOptions[planName];
        var setFeaturesRequest = new SetApplicationFeaturesRequest();
        setFeaturesRequest.EventLoggingIsEnabled = features.EventLoggingIsEnabled;
        setFeaturesRequest.EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod;
        foreach (Application app in apps)
        {
            app.BillingSubscriptionItemId = lineItem.Id;
            app.BillingPriceId = priceId;
            app.BillingPlan = planName;
            await _passwordlessClient.SetFeaturesAsync(app.Id, setFeaturesRequest);
        }

        await db.SaveChangesAsync();
    }

    private async Task<Subscription> GetSubscription(string subscriptionId)
    {
        var service = new SubscriptionService();
        var sub = await service.GetAsync(subscriptionId);
        return sub;
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