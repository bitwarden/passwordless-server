using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.Organization;

public class CreateApplicationModel : PageModel
{
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly IOptionsSnapshot<PasswordlessOptions> _passwordlessOptions;
    private readonly IApplicationService _applicationService;
    private readonly IDataService _dataService;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly BillingOptions _billingOptions;
    private readonly ISharedBillingService _billingService;

    public CreateApplicationModel(
        IOptionsSnapshot<PasswordlessOptions> passwordlessOptions,
        SignInManager<ConsoleAdmin> signInManager,
        IApplicationService applicationService,
        IDataService dataService,
        IPasswordlessManagementClient managementClient,
        IOptionsSnapshot<BillingOptions> billingOptions, ISharedBillingService billingService)
    {
        _dataService = dataService;
        _applicationService = applicationService;
        _managementClient = managementClient;
        _billingService = billingService;
        _passwordlessOptions = passwordlessOptions;
        _signInManager = signInManager;
        _billingOptions = billingOptions.Value;
    }

    public CreateApplicationForm Form { get; } = new();

    public Models.Organization Organization { get; set; }

    public ICollection<AvailablePlan> AvailablePlans { get; private set; } = new List<AvailablePlan>();

    public async Task<IActionResult> OnGet()
    {
        await InitializeAsync();
        if (!Organization.HasSubscription)
        {
            Form.Plan = _billingOptions.Store.Free;
        }

        if (Organization.Applications.Count >= Organization.MaxApplications)
        {
            return RedirectToPage("/billing/manage");
        }

        return Page();
    }

    public bool CanCreateApplication { get; set; }

    public async Task<IActionResult> OnPost(CreateApplicationForm form)
    {
        await InitializeAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Application app = new()
        {
            Id = form.Id,
            Name = form.Name,
            Description = form.Description,
            CreatedAt = DateTime.UtcNow,
            OrganizationId = User.GetOrgId().Value
        };

        string email = User.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        // validate we're allowed to create more Orgs
        if (!await _dataService.AllowedToCreateApplicationAsync())
        {
            ModelState.AddModelError("MaxApplications", "You have reached the maximum number of applications for your organization. Please upgrade to a paid organization");
            return Page();
        }

        // Attach a plan
        app.BillingPlan = form.Plan;

        if (form.Plan != _billingOptions.Store.Free)
        {
            var subItem = await _billingService.CreateSubscriptionItem(Organization, form.Plan);

            app.BillingSubscriptionItemId = subItem.subscriptionItemId;
            app.BillingPriceId = subItem.priceId;
        }

        CreateAppResultDto res;
        try
        {
            var features = _billingOptions.Plans[form.Plan].Features;
            var newAppOptions = new CreateAppDto
            {
                AdminEmail = email,
                EventLoggingIsEnabled = features.EventLoggingIsEnabled,
                EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod,
                MagicLinkEmailMonthlyQuota = features.MagicLinkEmailMonthlyQuota,
                MaxUsers = features.MaxUsers,
                AllowAttestation = features.AllowAttestation
            };
            res = await _managementClient.CreateApplicationAsync(app.Id, newAppOptions);
        }
        catch (Exception e)
        {
            ModelState.AddModelError("api.failure", "Failed to create your application");
            return Page();
        }

        if (string.IsNullOrEmpty(res.ApiKey1))
        {
            ModelState.AddModelError("ApiCall", res.Message);
            return Page();
        }

        // TODO: Get "Admin Console Keys" and "Real Keys"
        app.ApiKey = res.ApiKey2;
        app.ApiSecret = res.ApiSecret2;
        app.ApiUrl = _passwordlessOptions.Value.ApiUrl;
        app.Onboarding = new Onboarding
        {
            ApiKey = res.ApiKey1,
            ApiSecret = res.ApiSecret1,
            SensitiveInfoExpireAt = DateTime.UtcNow.AddDays(7)
        };

        await _applicationService.CreateApplicationAsync(app);

        var myUser = await _signInManager.UserManager.GetUserAsync(User);
        await _signInManager.RefreshSignInAsync(myUser);

        // TODO: Pass parameters in a better way
        return RedirectToPage("/App/Onboarding/GetStarted", new ApplicationPageRoutingContext(app.Id));
    }

    private async Task InitializeAsync()
    {
        CanCreateApplication = await _dataService.AllowedToCreateApplicationAsync();
        Organization = await _dataService.GetOrganizationWithDataAsync();

        if (Organization.HasSubscription)
        {
            AvailablePlans.Add(new AvailablePlan(_billingOptions.Store.Pro, _billingOptions.Plans[_billingOptions.Store.Pro].Ui.Label));
            AvailablePlans.Add(new AvailablePlan(_billingOptions.Store.Enterprise, _billingOptions.Plans[_billingOptions.Store.Enterprise].Ui.Label));
        }
    }

    public class CreateApplicationForm
    {
        [Required, MaxLength(60), MinLength(3), RegularExpression("^[a-zA-Z]{1}[a-zA-Z0-9 ]{2,59}$")]
        public string Name { get; set; }

        [Required, MaxLength(62), MinLength(3), RegularExpression("^[a-z]{1}[a-z0-9]{2,61}$")]
        public string Id { get; set; }

        [Required, MaxLength(120), MinLength(3), RegularExpression("^[a-zA-Z]{1}[a-zA-Z0-9 ]{2,119}$")]
        public string Description { get; set; }

        [Required]
        public string Plan { get; set; }
    }

    public record AvailablePlan(string Id, string Label);
}