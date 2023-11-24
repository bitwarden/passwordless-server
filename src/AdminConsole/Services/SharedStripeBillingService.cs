using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;
using Stripe.Checkout;

namespace Passwordless.AdminConsole.Services;

public class SharedStripeBillingService<TDbContext> : BaseBillingService<TDbContext>, ISharedBillingService where TDbContext : ConsoleDbContext
{   
    public SharedStripeBillingService(
        IDbContextFactory<TDbContext> dbContextFactory,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<SharedStripeBillingService<TDbContext>> logger,
        IOptions<StripeOptions> stripeOptions) : base(dbContextFactory, dataService, passwordlessClient, logger, stripeOptions)
    {
    }

    /// <inheritdoc />
    public async Task UpdateUsageAsync()
    {
        List<UsageItem> items = await GetUsageItems();

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

        // we only have one item per subscription
        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.GetAsync(subscriptionId);
        SubscriptionItem lineItem = subscription.Items.Data.Single();
        var planName = _stripeOptions.Plans.Single(x => x.Value.PriceId == lineItem.Price.Id).Key;

        await SetFeatures(customerId, planName, orgId, subscription.Id, subscription.Created, lineItem.Id, lineItem.Price.Id);
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

    public async Task<string> CreateCheckoutSessionAsync(
        int organizationId,
        string? billingCustomerId,
        string email,
        string planName,
        string successUrl,
        string cancelUrl)
    {
        if (_stripeOptions.Plans.All(x => x.Key != planName))
        {
            throw new ArgumentException("Invalid plan name");
        }

        var options = new SessionCreateOptions
        {
            Metadata =
                new Dictionary<string, string>
                {
                    { "orgId", organizationId.ToString() },
                    { "passwordless", "passwordless" }
                },
            ClientReferenceId = organizationId.ToString(),
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = _stripeOptions.Plans[planName].PriceId,
                }
            }
        };

        if (billingCustomerId != null)
        {
            options.Customer = billingCustomerId;
        }
        else
        {
            options.TaxIdCollection = new SessionTaxIdCollectionOptions { Enabled = true, };
            options.CustomerEmail = email;
        }

        var service = new SessionService();
        Session? session = await service.CreateAsync(options);

        return session.Url;
    }

    /// <inheritdoc />
    public async Task OnPostApplicationDeletedAsync(string subscriptionItemId)
    {
        var organization = await _dataService.GetOrganizationWithDataAsync();
        var isSubscriptionItemInUse = organization.Applications.Any(x => x.BillingSubscriptionItemId == subscriptionItemId);
        if (!isSubscriptionItemInUse)
        {
            if (organization.Applications.Any())
            {
                // If we have applications, then we can delete the subscription item,
                // as Stripe requires at least one active subscription item in a subscription.
                var service = new SubscriptionItemService();
                var options = new SubscriptionItemDeleteOptions();
                options.ClearUsage = true;
                await service.DeleteAsync(subscriptionItemId, options);
            }
            else
            {
                var subscriptionItemService = new SubscriptionItemService();
                var subscriptionItem = await subscriptionItemService.GetAsync(subscriptionItemId);
                var subscriptionService = new SubscriptionService();
                var cancelOptions = new SubscriptionCancelOptions();
                cancelOptions.Prorate = false;
                cancelOptions.InvoiceNow = true;
                await subscriptionService.CancelAsync(subscriptionItem.Subscription);
            }
        }
    }
}

