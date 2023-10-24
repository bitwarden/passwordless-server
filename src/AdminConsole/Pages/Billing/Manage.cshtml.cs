using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
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
        
        
        Plans = new List<PricingCardModel>
        {
            new(PlanConstants.Free, stripeOptions.Value.Plans[PlanConstants.Free]),
            new(PlanConstants.Pro, stripeOptions.Value.Plans[PlanConstants.Pro]),
            new(PlanConstants.Enterprise, stripeOptions.Value.Plans[PlanConstants.Enterprise])
        };
    }
    
    public List<Application> Applications { get; set; }
    
    public Models.Organization Organization { get; set; }
    
    public IReadOnlyCollection<PricingCardModel> Plans { get; init; }

    public async Task OnGet()
    {
        Applications = await _dataService.GetApplicationsAsync();
        Organization = await _dataService.GetOrganizationAsync();
        await OnLoadAsync();
    }
    
    public async Task<IActionResult> OnPostSubscribe(string planName)
    {
        if (_stripeOptions.Value.Plans.All(x => x.Key != planName))
        {
            throw new ArgumentException("Invalid plan name");
        }
        
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
                    { "orgId", orgId.ToString() },
                    { "passwordless", "passwordless" }
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
                    Price = _stripeOptions.Value.Plans[planName].PriceId,
                }
            }
        };

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
    
    private async Task OnLoadAsync()
    {
        // Set the active plan
        var activePlan = Organization.BillingPlan;
        Plans.Single(x => x.Name == activePlan).IsActive = true;
    }

    public class PricingCardModel
    {
        private static readonly CultureInfo PriceFormat = new("en-US");

        public PricingCardModel(
            string name,
            StripePlanOptions plan)
        {
            Name = name;
            Plan = plan;
        }
    
        public string Name { get; }
    
        public StripePlanOptions Plan { get; }
    
        public bool IsActive { get; set; }

        public string GetPrice() => $"{Plan.Price.ToString("C", PriceFormat)}";
    }
}