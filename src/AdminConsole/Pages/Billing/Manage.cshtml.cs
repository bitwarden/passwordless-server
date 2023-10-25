using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Stripe.Checkout;

namespace Passwordless.AdminConsole.Pages.Billing;

public class Manage : PageModel
{
    private readonly ISharedBillingService _billingService;
    private readonly IDataService _dataService;
    private readonly IOptions<StripeOptions> _stripeOptions;

    public Manage(ISharedBillingService billingService, IDataService dataService, IOptions<StripeOptions> stripeOptions)
    {
        _billingService = billingService;
        _dataService = dataService;
        _stripeOptions = stripeOptions;
    }

    public List<Application> Applications { get; set; }
    public Models.Organization Organization { get; set; }

    public async Task OnGet()
    {
        Applications = await _dataService.GetApplicationsAsync();
        Organization = await _dataService.GetOrganizationAsync();
    }

    public async Task<IActionResult> OnPost()
    {
        var orgId = User.GetOrgId();

        var customerEmail = User.GetEmail();

        var successUrl = Url.PageLink("/Billing/Success");
        successUrl += "?session_id={CHECKOUT_SESSION_ID}";

        var cancelUrl = Url.PageLink("/Billing/Cancelled");
        var options = new SessionCreateOptions
        {
            CustomerEmail = customerEmail,
            Metadata =
                new Dictionary<string, string>
                {
                    { "orgId", orgId.ToString() }, { "passwordless", "passwordless" }
                },
            TaxIdCollection = new SessionTaxIdCollectionOptions
            {
                Enabled = true,
            },
            ClientReferenceId = orgId.ToString(),
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = _stripeOptions.Value.UsersProPriceId,
                }
            }
        };

        var service = new SessionService();
        Session? session = await service.CreateAsync(options);

        return Redirect(session.Url);
    }

    public async Task<IActionResult> OnPostPortal()
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
}