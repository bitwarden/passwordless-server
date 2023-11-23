using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;
using Stripe;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.Billing;

public class Manage : BaseExtendedPageModel
{
    private readonly ISharedBillingService _billingService;
    private readonly IDataService _dataService;
    private readonly IOptions<StripeOptions> _stripeOptions;
    private readonly IBillingHelper _billingHelper;

    public Manage(ISharedBillingService billingService, IDataService dataService, IOptions<StripeOptions> stripeOptions, IBillingHelper billingHelper)
    {
        _billingService = billingService;
        _dataService = dataService;
        _stripeOptions = stripeOptions;
        _billingHelper = billingHelper;

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
            PaymentMethods = await _billingHelper.GetPaymentMethods(Organization.BillingCustomerId);
        }
    }

    public async Task<IActionResult> OnPostUpgradePro()
    {
        var redirect = await _billingHelper.UpgradeOrganization();
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

    public record PricingCardModel(
        string Name,
        StripePlanOptions Plan)
    {
        /// <summary>
        /// We want to display the price in US dollars.
        /// </summary>
        private static readonly CultureInfo PriceFormat = new("en-US");

        /// <summary>
        /// Indicates if the plan is the active plan for the organization.
        /// </summary>
        public bool IsActive { get; set; }
    }

    public record PaymentMethodModel(string Brand, string Number, DateTime ExpirationDate)
    {
        public string CardIcon
        {
            get
            {
                var path = new StringBuilder("Shared/Icons/PaymentMethods/");
                switch (Brand)
                {
                    case "amex":
                        path.Append("Amex");
                        break;
                    case "diners":
                        path.Append("Diners");
                        break;
                    case "discover":
                        path.Append("Discover");
                        break;
                    case "jcb":
                        path.Append("Jcb");
                        break;
                    case "mastercard":
                        path.Append("MasterCard");
                        break;
                    case "unionpay":
                        path.Append("UnionPay");
                        break;
                    case "visa":
                        path.Append("Visa");
                        break;
                    default:
                        path.Append("UnknownCard");
                        break;
                }
                return path.ToString();
            }
        }
    }
}