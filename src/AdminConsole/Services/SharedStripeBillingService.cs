using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Extensions;
using Stripe;
using Stripe.Checkout;

namespace Passwordless.AdminConsole.Services;

public class SharedStripeBillingService : BaseBillingService, ISharedBillingService
{
    private readonly IScopedPasswordlessClient _api;
    public SharedStripeBillingService(
        ConsoleDbContext db,
        IDataService dataService,
        IPasswordlessManagementClient passwordlessClient,
        ILogger<SharedStripeBillingService> logger,
        IOptions<BillingOptions> billingOptions,
        IHttpContextAccessor httpContextAccessor,
        IScopedPasswordlessClient api) : base(
        db,
        dataService,
        passwordlessClient,
        logger,
        billingOptions,
        httpContextAccessor)
    {
        _api = api;
    }

    public async Task<(string subscriptionItemId, string priceId)> CreateSubscriptionItem(Organization org, string planSKU)
    {
        if (org.BillingSubscriptionId == null)
        {
            throw new InvalidOperationException("Cannot create a paid application without a subscription");
        }
        var subscriptionItemService = new SubscriptionItemService();
        var listOptions = new SubscriptionItemListOptions { Subscription = org.BillingSubscriptionId };
        var subscriptionItems = await subscriptionItemService.ListAsync(listOptions);

        var subscriptionItem = subscriptionItems.SingleOrDefault(x => x.Price.Id == _billingOptions.Plans[planSKU].PriceId);
        if (subscriptionItem == null)
        {
            var createOptions = new SubscriptionItemCreateOptions
            {
                Subscription = org.BillingSubscriptionId,
                Price = _billingOptions.Plans[planSKU].PriceId,
                ProrationDate = DateTime.UtcNow,
                ProrationBehavior = "create_prorations"
            };
            subscriptionItem = await subscriptionItemService.CreateAsync(createOptions);
        }

        return (subscriptionItem.Id, subscriptionItem.Price.Id);
    }

    public async Task<string> GetManagementUrl(int orgId)
    {
        var customerId = await this.GetCustomerIdAsync(orgId);
        var returnUrl = _httpContextAccessor.HttpContext!.Request.GetBaseUrl() + "/billing/manage";

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = returnUrl,

        };
        var service = new Stripe.BillingPortal.SessionService();
        Stripe.BillingPortal.Session? session = await service.CreateAsync(options);

        return session.Url;
    }

    public async Task<string?> GetRedirectToUpgradeOrganization(string? selectedPlan = null)
    {
        selectedPlan ??= _billingOptions.Store.Pro;

        var organization = await _dataService.GetOrganizationWithDataAsync();
        if (!organization.HasSubscription)
        {
            var successUrl = _httpContextAccessor.HttpContext!.Request.GetBaseUrl() + "/billing/success";
            successUrl += "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = _httpContextAccessor.HttpContext!.Request.GetBaseUrl() + "/billing/cancelled";
            var sessionUrl = await this.CreateCheckoutSessionAsync(organization.Id, organization.BillingCustomerId, _httpContextAccessor.HttpContext.User.GetEmail(), selectedPlan, successUrl, cancelUrl);
            return sessionUrl;
        }

        return null;
    }

    public async Task<string> ChangePlanAsync(string app, string selectedPlan)
    {
        var redirectToUpgrade = await this.GetRedirectToUpgradeOrganization(selectedPlan);
        if (redirectToUpgrade != null) return redirectToUpgrade;

        var organization = await _dataService.GetOrganizationWithDataAsync();

        var application = organization.Applications.SingleOrDefault(x => x.Id == app);
        var existingSubscriptionItemId = application.BillingSubscriptionItemId;

        var plan = _billingOptions.Plans[selectedPlan];
        var priceId = plan.PriceId!;

        var subscriptionItemService = new SubscriptionItemService();
        var subscriptionItemOptions = new SubscriptionItemListOptions
        {
            Subscription = organization.BillingSubscriptionId
        };
        var subscriptionItems = await subscriptionItemService.ListAsync(subscriptionItemOptions);

        // Create the new subscription item if it doesn't exist.
        string? subscriptionItemId = subscriptionItems.SingleOrDefault(x => x.Price.Id == priceId)?.Id;
        if (subscriptionItemId == null)
        {
            var createSubscriptionItemOptions = new SubscriptionItemCreateOptions
            {
                Price = priceId,
                ProrationDate = DateTime.UtcNow,
                ProrationBehavior = "create_prorations",
                Subscription = organization.BillingSubscriptionId
            };
            try
            {
                var createSubscriptionItemResult = await subscriptionItemService.CreateAsync(createSubscriptionItemOptions);
                subscriptionItemId = createSubscriptionItemResult.Id;
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Failed to create subscription item for organization {OrganizationId}: {Message}", organization.Id, e.StripeError.Message);
                throw;
            }
        }

        // Delete subscription item if it's not used by any other application inside this organization.
        if (!organization.Applications.Any(x => x.Id != app && x.BillingSubscriptionItemId == existingSubscriptionItemId))
        {
            var deleteSubscriptionItemOptions = new SubscriptionItemDeleteOptions { ClearUsage = true };
            try
            {
                await subscriptionItemService.DeleteAsync(existingSubscriptionItemId, deleteSubscriptionItemOptions);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to delete subscription item {SubscriptionItemId}", existingSubscriptionItemId);
                throw;
            }
        }

        await this.SetPlanOnApp(app, selectedPlan, subscriptionItemId, priceId);

        return "/billing/manage";
    }

    public async Task<IReadOnlyCollection<PaymentMethodModel>> GetPaymentMethods(string? organizationBillingCustomerId)
    {
        var paymentMethodsService = new CustomerService();
        var paymentMethods = await paymentMethodsService.ListPaymentMethodsAsync(organizationBillingCustomerId);
        if (paymentMethods != null)
        {
            return paymentMethods.Data
                .Where(x => x.Type == "card")
                .Select(x =>
                    new PaymentMethodModel(
                        x.Card.Brand,
                        x.Card.Last4,
                        new DateTime((int)x.Card.ExpYear, (int)x.Card.ExpMonth, 1)))
                .ToImmutableList();
        }

        return Array.Empty<PaymentMethodModel>().ToImmutableList();
    }

    public async Task<IReadOnlyCollection<Organization>> GetPayingOrganizationsAsync()
    {
        var organizations = await Db.Organizations
            .AsNoTracking()
            .Include(x => x.Applications)
            .Where(x => x.BillingCustomerId != null)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        return organizations;
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
        var planName = _billingOptions.Plans.Single(x => x.Value.PriceId == lineItem.Price.Id).Key;

        await UpgradeToPaidOrganization(customerId, planName, orgId, subscription.Id, subscription.Created, lineItem.Id, lineItem.Price.Id);
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

    public async Task<IReadOnlyCollection<InvoiceModel>> GetInvoicesAsync()
    {
        var organization = await _dataService.GetOrganizationAsync();
        if (string.IsNullOrEmpty(organization.BillingCustomerId))
        {
            return new List<InvoiceModel>(0);
        }

        var listRequest = new InvoiceListOptions
        {
            Customer = organization.BillingCustomerId,
            Limit = 100
        };

        var invoiceService = new InvoiceService();
        var invoicesResult = await invoiceService.ListAsync(listRequest);

        if (invoicesResult?.Data == null)
        {
            return new List<InvoiceModel>(0);
        }

        return invoicesResult.Data
            .Where(x => x.InvoicePdf != null)
            .Select(x => new InvoiceModel
            {
                Number = x.Number,
                Date = x.Created,
                Amount = $"{(x.Total / 100.0M):N2} {x.Currency.ToUpperInvariant()}",
                Pdf = x.InvoicePdf,
                Paid = x.Paid
            }).ToImmutableList();
    }

    private async Task<string> CreateCheckoutSessionAsync(
        int organizationId,
        string? billingCustomerId,
        string email,
        string planName,
        string successUrl,
        string cancelUrl)
    {
        if (_billingOptions.Plans.All(x => x.Key != planName))
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
                    Price = _billingOptions.Plans[planName].PriceId,
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