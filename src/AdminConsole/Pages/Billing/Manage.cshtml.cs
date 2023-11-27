using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.Billing;

public class Manage : BaseExtendedPageModel
{
    private readonly ISharedBillingService _billingService;
    private readonly IDataService _dataService;
    private readonly IOptions<BillingOptions> _billingOptions;

    public Manage(ISharedBillingService billingService, IDataService dataService, IOptions<BillingOptions> billingOptions)
    {
        _billingService = billingService;
        _dataService = dataService;
        _billingOptions = billingOptions;

        var plans = new List<PricingCardModel>
        {
            new PricingCardModel(_billingOptions.Value.Store.Free, _billingOptions.Value.Plans[_billingOptions.Value.Store.Free]),
            new PricingCardModel(_billingOptions.Value.Store.Pro, _billingOptions.Value.Plans[_billingOptions.Value.Store.Pro]),
            new PricingCardModel(_billingOptions.Value.Store.Enterprise, _billingOptions.Value.Plans[_billingOptions.Value.Store.Enterprise])
        };
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
            .Select(x => ApplicationModel.FromEntity(x, _billingOptions.Value.Plans[x.BillingPlan]))
            .ToList();
        Organization = await _dataService.GetOrganizationAsync();

        if (Organization.HasSubscription)
        {
            PaymentMethods = await _billingService.GetPaymentMethods(Organization.BillingCustomerId);
        }
    }

    public async Task<IActionResult> OnPostUpgradePro()
    {
        var redirect = await _billingService.GetRedirectToUpgradeOrganization(_billingOptions.Value.Store.Pro);
        return Redirect(redirect);
    }

    public async Task<IActionResult> OnPostManage()
    {
        var manageUrl = await _billingService.GetManagementUrl(User.GetOrgId().Value);
        return Redirect(manageUrl);
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
        public static ApplicationModel FromEntity(Application entity, BillingPlanOptions options)
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