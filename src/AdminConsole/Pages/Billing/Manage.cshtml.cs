using AdminConsole.Billing;
using AdminConsole.Db;
using AdminConsole.Helpers;
using AdminConsole.Models;
using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Stripe.Checkout;

namespace AdminConsole.Pages.Billing;

public class Manage : PageModel
{
    private readonly ConsoleDbContext _context;
    private readonly DataService _dataService;
    private readonly IOptions<StripeOptions> _stripeOptions;

    public Manage(ConsoleDbContext context, DataService dataService, IOptions<StripeOptions> stripeOptions)
    {
        _context = context;
        _dataService = dataService;
        _stripeOptions = stripeOptions;
    }

    public List<Application> Applications { get; set; }
    public Models.Organization Organization { get; set; }

    public async Task OnGet()
    {
        Applications = await _dataService.GetApplications();
        Organization = await _dataService.GetOrganization();
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
        var customerId = _context.Organizations.Where(o => o.Id == User.GetOrgId()).Select(o => o.BillingCustomerId)
            .FirstOrDefault();
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