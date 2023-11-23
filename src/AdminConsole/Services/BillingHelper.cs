using System.Collections.Immutable;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Pages.Billing;
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

    public async Task<string> UpgradeOrganization()
    {
        var urlBuilder = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

        var organization = await _dataService.GetOrganizationWithDataAsync();
        if (!organization.HasSubscription)
        {
            var successUrl = urlBuilder.PageLink("/Billing/Success");
            successUrl += "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = urlBuilder.PageLink("/Billing/Cancelled");
            var sessionUrl = await _billingService.CreateCheckoutSessionAsync(organization.Id, organization.BillingCustomerId, _actionContextAccessor.ActionContext.HttpContext.User.GetEmail(), _stripeOptions.Store.Pro, successUrl, cancelUrl);
            return sessionUrl;
        }

        throw new InvalidOperationException("Organization already has a subscription.");
    }

    public async Task<IReadOnlyCollection<Manage.PaymentMethodModel>> GetPaymentMethods(string? organizationBillingCustomerId)
    {
        var paymentMethodsService = new CustomerService();
        var paymentMethods = await paymentMethodsService.ListPaymentMethodsAsync(organizationBillingCustomerId);
        if (paymentMethods != null)
        {
            return paymentMethods.Data
                .Where(x => x.Type == "card")
                .Select(x =>
                    new Manage.PaymentMethodModel(
                        x.Card.Brand,
                        x.Card.Last4,
                        new DateTime((int)x.Card.ExpYear, (int)x.Card.ExpMonth, 1)))
                .ToImmutableList();
        }

        return Array.Empty<Manage.PaymentMethodModel>().ToImmutableList();
    }

    public bool IsBillingEnabled { get; set; } = true;
}

public class NoopBillingHelper<TDbContext> : IBillingHelper where TDbContext : ConsoleDbContext
{
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ISharedBillingService _sharedBillingService;
    private readonly IDataService _dataService;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly StripeOptions _stripeOptions;

    public NoopBillingHelper(
        IPasswordlessManagementClient managementClient, IOptions<StripeOptions> stripeOptions, ISharedBillingService sharedBillingService,IDataService dataService, IDbContextFactory<TDbContext> dbContextFactory, IActionContextAccessor actionContextAccessor)
    {
        _managementClient = managementClient;
        _sharedBillingService = sharedBillingService;
        _dataService = dataService;
        _dbContextFactory = dbContextFactory;
        _actionContextAccessor = actionContextAccessor;
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

    public async Task<string> UpgradeOrganization()
    {
        
        var orgId = _actionContextAccessor.ActionContext.HttpContext?.User?.GetOrgId();
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        
        var org = await db.Organizations.FirstOrDefaultAsync(x => x.Id == orgId);

        if (org == null)
        {
            throw new InvalidOperationException("Org not found");
        }

        if (org.HasSubscription)
        {
            return "/";
        }
        org.BillingCustomerId = "simple";
        org.BecamePaidAt = DateTime.Now;
        org.BillingSubscriptionId = "simple";

        var plan = _stripeOptions.Plans[_stripeOptions.Store.Pro];
                
        
        if (plan == null)
        {
            throw new InvalidOperationException("Received a subscription for a product that is not configured.");
        }
        
        var features = plan.Features;

        // Increase the limits
        org.MaxAdmins = features.MaxAdmins;
        org.MaxApplications = features.MaxApplications;



        
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
            application.BillingPlan = _stripeOptions.Store.Pro;
            application.BillingSubscriptionItemId = "simple";
            application.BillingPriceId = "simple";
            await _managementClient.SetFeaturesAsync(application.Id, setFeaturesRequest);
        }

        
        await db.SaveChangesAsync();

        return "/";
    }

    public async Task<IReadOnlyCollection<Manage.PaymentMethodModel>> GetPaymentMethods(string? organizationBillingCustomerId)
    {
        return Array.Empty<Manage.PaymentMethodModel>().ToImmutableList();
    }

    public bool IsBillingEnabled { get; set; } = false;
}

public interface IBillingHelper
{
    Task<string> ChangePlanASync(string app, string selectedPlan);
    Task<string> UpgradeOrganization();
    Task<IReadOnlyCollection<Manage.PaymentMethodModel>> GetPaymentMethods(string? organizationBillingCustomerId);
    bool IsBillingEnabled { get; set; } 
}