using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Stripe;

namespace Passwordless.AdminConsole.Services;

public class BillingHelper : IBillingHelper
{
    private readonly IDataService _dataService;
    private readonly ISharedBillingService _billingService;
    private readonly UrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly StripeOptions _stripeOptions;

    public BillingHelper(IDataService dataService, ISharedBillingService billingService, UrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IOptions<StripeOptions> stripeOptions, IPasswordlessManagementClient managementClient)
    {
        _dataService = dataService;
        _billingService = billingService;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        _managementClient = managementClient;
        _stripeOptions = stripeOptions.Value;
    }
  
    
    public async Task<string> ChangePlanASync(string app, string selectedPlan)
    {
        var organization = await _dataService.GetOrganizationWithDataAsync();
        if (!organization.HasSubscription)
        {
            var urlBuilder = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            
            var successUrl = urlBuilder.PageLink("/Billing/Success");
            successUrl += "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = urlBuilder.PageLink("/Billing/Cancelled");
            var sessionUrl = await _billingService.CreateCheckoutSessionAsync(organization.Id, organization.BillingCustomerId, _actionContextAccessor.ActionContext.HttpContext.User.GetEmail(), selectedPlan, successUrl, cancelUrl);
            return sessionUrl;
        }
        
        // Org has Subscription
        

        var application = organization.Applications.SingleOrDefault(x => x.Id == app);
        var existingSubscriptionItemId = application.BillingSubscriptionItemId;

        var plan = _stripeOptions.Plans[selectedPlan];
        var priceId = plan.PriceId!;
        var subscriptionItem = organization.Applications
            .Where(x => x.BillingPriceId == priceId)
            .GroupBy(x => new
            {
                x.BillingPriceId,
                x.BillingSubscriptionItemId
            })
            .Select(x => new
            {
                PriceId = x.Key.BillingPriceId!,
                Id = x.Key.BillingSubscriptionItemId!
            }).SingleOrDefault();

        var subscriptionItemService = new SubscriptionItemService();

        // Create subscription item if it doesn't exist.
        if (subscriptionItem == null)
        {
            var createSubscriptionItemOptions = new SubscriptionItemCreateOptions
            {
                Price = priceId,
                ProrationDate = DateTime.UtcNow,
                ProrationBehavior = "create_prorations",
                Subscription = organization.BillingSubscriptionId
            };
            var createSubscriptionItemResult = await subscriptionItemService.CreateAsync(createSubscriptionItemOptions);
            subscriptionItem = new
            {
                PriceId = createSubscriptionItemResult.Price.Id,
                Id = createSubscriptionItemResult.Id
            };
        }

        // Delete subscription item if it's not used by any other application inside this organization.
        if (!organization.Applications.Any(x => x.Id != app && x.BillingSubscriptionItemId == existingSubscriptionItemId))
        {
            var deleteSubscriptionItemOptions = new SubscriptionItemDeleteOptions { ClearUsage = true };
            await subscriptionItemService.DeleteAsync(existingSubscriptionItemId, deleteSubscriptionItemOptions);
        }

        await _billingService.UpdateApplicationAsync(app, selectedPlan, subscriptionItem.Id, priceId);

        var updateFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = plan.Features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = plan.Features.EventLoggingRetentionPeriod
        };
        await _managementClient.SetFeaturesAsync(app, updateFeaturesRequest);

        return "/billing/manage";
    }
}

public class NoopBillingHelper : IBillingHelper
{
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ISharedBillingService _sharedBillingService;
    private readonly StripeOptions _stripeOptions;

    public NoopBillingHelper(IPasswordlessManagementClient managementClient, IOptions<StripeOptions> stripeOptions, ISharedBillingService sharedBillingService)
    {
        _managementClient = managementClient;
        _sharedBillingService = sharedBillingService;
        _stripeOptions = stripeOptions.Value;
    }
    
    public async Task<string> ChangePlanASync(string app, string selectedPlan)
    {
        var plan = _stripeOptions.Plans[selectedPlan];

        var updateFeaturesRequest = new SetApplicationFeaturesRequest
        {
            EventLoggingIsEnabled = plan.Features.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = plan.Features.EventLoggingRetentionPeriod
        };
        
        await _sharedBillingService.UpdateApplicationAsync(app, selectedPlan, "simple", "simple");
        
        await _managementClient.SetFeaturesAsync(app, updateFeaturesRequest);

        return "/billing/manage";
    }
}

public interface IBillingHelper
{
    Task<string> ChangePlanASync(string app, string selectedPlan);
}