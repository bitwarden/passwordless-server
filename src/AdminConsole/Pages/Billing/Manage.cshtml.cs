using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.Billing;

public class Manage : BaseExtendedPageModel
{
    private readonly ISharedBillingService _billingService;
    private readonly IDataService _dataService;
    private readonly IOptions<StripeOptions> _stripeOptions;

    public Manage(ISharedBillingService billingService, IDataService dataService, IOptions<StripeOptions> stripeOptions)
    {
        _billingService = billingService;
        _dataService = dataService;
        _stripeOptions = stripeOptions;

        var plans = new List<PricingCardModel>();
        plans.Add(new PricingCardModel(_stripeOptions.Value.Store.Free, _stripeOptions.Value.Plans[_stripeOptions.Value.Store.Free]));
        plans.Add(new PricingCardModel(_stripeOptions.Value.Store.Pro, _stripeOptions.Value.Plans[_stripeOptions.Value.Store.Pro]));
        plans.Add(new PricingCardModel(_stripeOptions.Value.Store.Enterprise, _stripeOptions.Value.Plans[_stripeOptions.Value.Store.Enterprise]));
        Plans = plans;
    }

    public ICollection<ApplicationModel> Applications { get; set; }

    public Models.Organization Organization { get; set; }

    public IReadOnlyCollection<PricingCardModel> Plans { get; init; }

    public IReadOnlyCollection<PaymentMethodModel> PaymentMethods { get; private set; }

    public async Task OnGet()
    {
        var applications = await _dataService.GetApplicationsAsync();
        Applications = applications
            .Select(x => ApplicationModel.FromEntity(x, _stripeOptions.Value.Plans[x.BillingPlan]))
            .ToList();
        Organization = await _dataService.GetOrganizationAsync();

        if (Organization.HasSubscription)
        {
            PaymentMethods = await _billingService.GetPaymentMethods(Organization.BillingCustomerId);
        }
    }

    public async Task<IActionResult> OnPostUpgradePro()
    {
        var redirect = await _billingService.GetRedirectToUpgradeOrganization();
        return Redirect(redirect);
    }

    public async Task<IActionResult> OnPostManage()
    {
        var customerId = await _billingService.GetCustomerIdAsync(User.GetOrgId().Value);
        var returnUrl = Url.PageLink("/Billing/Manage");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = returnUrl,

        };
        var service = new Stripe.BillingPortal.SessionService();
        Stripe.BillingPortal.Session? session = await service.CreateAsync(options);

        return Redirect(session.Url);
    }

    public IActionResult OnPostChangePlan(string id)
    {
        return RedirectToApplicationPage("/App/Settings/Settings", new ApplicationPageRoutingContext(id));
    }

    public record ApplicationModel(
        string Id,
        string Description,
        int Users,
        string Plan,
        bool CanChangePlan)
    {
        public static ApplicationModel FromEntity(Application entity, StripePlanOptions options)
        {
            var canChangePlan = !entity.DeleteAt.HasValue;
            return new ApplicationModel(
                entity.Id,
                entity.Description,
                entity.CurrentUserCount,
                options.Ui.Label,
                canChangePlan);
        }
    }

}