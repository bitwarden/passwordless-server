using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Configuration;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using NewAppOptions = Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts.NewAppOptions;
using NewAppResponse = Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts.NewAppResponse;

namespace Passwordless.AdminConsole.Pages.Organization;

public class CreateApplicationModel : PageModel
{
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly IOptionsSnapshot<PasswordlessOptions> _passwordlessOptions;
    private readonly IApplicationService _applicationService;
    private readonly IDataService _dataService;
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly PlansOptions _plansOptions;

    public CreateApplicationModel(
        IOptionsSnapshot<PasswordlessOptions> passwordlessOptions,
        SignInManager<ConsoleAdmin> signInManager,
        IApplicationService applicationService,
        IDataService dataService,
        IPasswordlessManagementClient managementClient,
        IOptionsSnapshot<PlansOptions> plansOptions)
    {
        _dataService = dataService;
        _applicationService = applicationService;
        _managementClient = managementClient;
        _passwordlessOptions = passwordlessOptions;
        _signInManager = signInManager;
        _plansOptions = plansOptions.Value;
    }

    public CreateApplicationForm Form { get; set; }

    public async Task OnGet()
    {
        CanCreateApplication = await _dataService.AllowedToCreateApplicationAsync();
    }

    public bool CanCreateApplication { get; set; }

    public async Task<IActionResult> OnPost(CreateApplicationForm form)
    {
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
        var org = await _dataService.GetOrganizationAsync();

        NewAppResponse res;
        try
        {
            var plan = _plansOptions[org.BillingPlan];
            var newAppOptions = new NewAppOptions
            {
                AdminEmail = email,
                EventLoggingIsEnabled = plan.EventLoggingIsEnabled,
                EventLoggingRetentionPeriod = plan.EventLoggingRetentionPeriod
            };
            res = await _managementClient.CreateApplication(app.Id, newAppOptions);
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
        return RedirectToPage("/App/Onboarding/GetStarted", new { app = app.Id });
    }

    public class CreateApplicationForm
    {
        [Required, MaxLength(60)]
        public string Name { get; set; }
        [Required, MaxLength(62)]
        public string Id { get; set; }
        [Required, MaxLength(120)]
        public string Description { get; set; }
    }
}