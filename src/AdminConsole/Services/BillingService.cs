using AdminConsole.Billing;
using AdminConsole.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services;
using Stripe;
using Application = AdminConsole.Models.Application;

namespace AdminConsole.Services;

public class SharedBillingService
{
    private readonly ConsoleDbContext _dbContext;
    private readonly IPasswordlessManagementClient _passwordlessClient;
    private readonly PlansOptions _plansOptions;
    private readonly ILogger<SharedBillingService> _logger;
    private readonly StripeOptions _stripeOptions;

    public SharedBillingService(
        ConsoleDbContext dbContext,
        IPasswordlessManagementClient passwordlessClient,
        IOptionsSnapshot<PlansOptions> plansOptions,
        ILogger<SharedBillingService> logger,
        IOptions<StripeOptions> stripeOptions)
    {
        _dbContext = dbContext;
        _passwordlessClient = passwordlessClient;
        _plansOptions = plansOptions.Value;
        _logger = logger;
        _stripeOptions = stripeOptions.Value;
    }

    public async Task UpdateUsage()
    {
        List<Application> apps = await _dbContext.Applications.Where(x => x.BillingPriceId != null)
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
                await UpdateStripe(app.BillingSubscriptionItemId, users);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to update usage for app {appId}: {error}", app.Id, e.Message);
            }
        }
    }

    private async Task AddBillingSubscriptionItemId(Application app)
    {
        var org = await _dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == app.OrganizationId);
        Subscription subscription = await GetSubscription(org.BillingSubscriptionId);

        // Increase the limits
        org.MaxAdmins = 1000;
        org.MaxApplications = 1000;

        // get the lineItem from the subscription
        SubscriptionItem? lineItem = subscription.Items.Data.FirstOrDefault(i => i.Price.Id == app.BillingPriceId);
        app.BillingSubscriptionItemId = lineItem.Id;
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateStripe(string subscriptionItemId, int users)
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
        var org = await _dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == orgId);
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
        List<Application> apps = await _dbContext.Applications.Where(a => a.OrganizationId == orgId).ToListAsync();
        var features = _plansOptions[planName];
        var setFeaturesRequest = new SetApplicationFeaturesRequest();
        setFeaturesRequest.AuditLoggingIsEnabled = features.AuditLoggingIsEnabled;
        setFeaturesRequest.AuditLoggingRetentionPeriod = features.AuditLoggingRetentionPeriod;
        foreach (Application app in apps)
        {
            app.BillingSubscriptionItemId = lineItem.Id;
            app.BillingPriceId = priceId;
            app.BillingPlan = planName;
            await _passwordlessClient.SetFeaturesAsync(app.Id, setFeaturesRequest);
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task<Subscription> GetSubscription(string subscriptionId)
    {
        var service = new SubscriptionService();
        var sub = await service.GetAsync(subscriptionId);
        return sub;
    }

    public async Task UpdateSubscriptionStatus(Invoice? dataObject)
    {
        // todo: Handled paid or unpaid events
    }
}