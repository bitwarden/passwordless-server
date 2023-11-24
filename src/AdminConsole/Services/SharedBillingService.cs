using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;
using Stripe.Checkout;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Services;

public class SharedBillingService<TDbContext> : BaseBillingService<TDbContext>, ISharedBillingService where TDbContext : ConsoleDbContext
{   
    public SharedBillingService(
        IDbContextFactory<TDbContext> dbContextFactory,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<SharedBillingService<TDbContext>> logger,
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

public record UsageItem(string BillingSubscriptionItemId, int Users);