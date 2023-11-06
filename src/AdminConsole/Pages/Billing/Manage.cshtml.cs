using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;
using Stripe;
using Stripe.Checkout;
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


        Plans = new List<PricingCardModel>
        {
            new(PlanConstants.Free, stripeOptions.Value.Plans[PlanConstants.Free]),
            new(PlanConstants.Pro, stripeOptions.Value.Plans[PlanConstants.Pro]),
            new(PlanConstants.Enterprise, stripeOptions.Value.Plans[PlanConstants.Enterprise])
        };
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
            var paymentMethodsService = new CustomerService();
            var paymentMethods = await paymentMethodsService.ListPaymentMethodsAsync(Organization.BillingCustomerId);
            if (paymentMethods != null)
            {
                PaymentMethods = paymentMethods.Data
                    .Where(x => x.Type == "card")
                    .Select(x =>
                        new PaymentMethodModel(
                            x.Card.Brand,
                            x.Card.Last4,
                            new DateTime((int)x.Card.ExpYear, (int)x.Card.ExpMonth, 1)))
                    .ToImmutableList();
            }
        }
    }

    public async Task<IActionResult> OnPostSubscribe(string planName)
    {
        if (_stripeOptions.Value.Plans.All(x => x.Key != planName))
        {
            throw new ArgumentException("Invalid plan name");
        }

        var organization = await _dataService.GetOrganizationAsync();

        var successUrl = Url.PageLink("/Billing/Success");
        successUrl += "?session_id={CHECKOUT_SESSION_ID}";

        var cancelUrl = Url.PageLink("/Billing/Cancelled");
        var options = new SessionCreateOptions
        {
            Metadata =
                new Dictionary<string, string>
                {
                    { "orgId", organization.Id.ToString() },
                    { "passwordless", "passwordless" }
                },
            ClientReferenceId = organization.Id.ToString(),
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = _stripeOptions.Value.Plans[planName].PriceId,
                }
            }
        };

        if (organization.BillingCustomerId != null)
        {
            options.Customer = organization.BillingCustomerId;
        }
        else
        {
            options.TaxIdCollection = new SessionTaxIdCollectionOptions { Enabled = true, };
            options.CustomerEmail = User.GetEmail();
        }

        var service = new SessionService();
        Session? session = await service.CreateAsync(options);

        return Redirect(session.Url);
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
        string Plan)
    {
        public static ApplicationModel FromEntity(Application entity, StripePlanOptions options)
        {
            return new ApplicationModel(
                entity.Id,
                entity.Description,
                entity.CurrentUserCount,
                options.Ui.Label);
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