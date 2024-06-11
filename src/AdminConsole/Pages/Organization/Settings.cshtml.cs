using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.Mail;

namespace Passwordless.AdminConsole.Pages.Organization;

public class SettingsModel : PageModel
{
    private readonly IDataService _dataService;
    private readonly ISharedBillingService _billingService;
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly IMailService _mailService;
    private readonly ISystemClock _systemClock;
    private readonly ILogger<SettingsModel> _logger;
    private readonly IOptions<PasswordlessOptions> _options;
    private readonly IAuthorizationService _authorizationService;

    public SettingsModel(
        IDataService dataService,
        ISharedBillingService billingService,
        SignInManager<ConsoleAdmin> signInManager,
        IMailService mailService,
        ISystemClock systemClock,
        ILogger<SettingsModel> logger,
        IOptions<PasswordlessOptions> options,
        IAuthorizationService authorizationService)
    {
        _dataService = dataService;
        _billingService = billingService;
        _signInManager = signInManager;
        _mailService = mailService;
        _systemClock = systemClock;
        _logger = logger;
        _options = options;
        _authorizationService = authorizationService;
    }

    public async Task OnGet()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        var organization = await _dataService.GetOrganizationWithDataAsync();
        Name = organization.Name;
        ApplicationsCount = organization.Applications.Count;

        UserHasClaim = (await _authorizationService.AuthorizeAsync(User, CustomPolicy.StepUp)).Succeeded;
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        _logger.LogInformation("Deleted Org");
        // await LoadData();
        //
        // var username = User.Identity?.Name ?? throw new InvalidOperationException();
        // if (!string.Equals(Name, NameConfirmation, StringComparison.Ordinal))
        // {
        //     ModelState.AddModelError(string.Empty, "Name confirmation does not match.");
        //     return Page();
        // }
        //
        // var organization = await _dataService.GetOrganizationWithDataAsync();
        // var emails = organization.Admins.Select(x => x.Email).ToList();
        // await _mailService.SendOrganizationDeletedAsync(organization.Name, emails, username, _systemClock.UtcNow.UtcDateTime);
        //
        // if (organization.HasSubscription)
        // {
        //     var isSubscriptionDeleted = await _billingService.CancelSubscriptionAsync(organization.BillingSubscriptionId!);
        //     if (!isSubscriptionDeleted)
        //     {
        //         _logger.LogError(
        //             "Organization {orgId} tried to cancel subscription {subscriptionId}, but failed.",
        //             organization.Name,
        //             organization.BillingSubscriptionId);
        //         throw new Exception("Failed to cancel subscription.");
        //     }
        // }
        //
        // var isDeleted = await _dataService.DeleteOrganizationAsync();
        // if (isDeleted)
        // {
        //     await _signInManager.SignOutAsync();
        // }
        //
        // return RedirectToPage();
        return null;
    }

    /// <summary>
    /// Organization Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The organization's name which is confirmed by the end user to allow the organization to be deleted.
    /// </summary>
    [BindProperty]
    public string NameConfirmation { get; set; } = string.Empty;

    /// <summary>
    /// Whether the organization can be deleted.
    /// </summary>
    public bool CanDelete => true;

    /// <summary>
    /// The amount of active applications belonging to the organization.
    /// </summary>
    public int ApplicationsCount { get; set; }

    public string ApiUrl => _options.Value.ApiUrl;
    public string ApiKey => _options.Value.ApiKey!;
    public string UserId => User.GetId();
    public string Purpose => "step-up";

    public bool UserHasClaim { get; set; }
}