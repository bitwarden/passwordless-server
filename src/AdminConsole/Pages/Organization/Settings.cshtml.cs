using AdminConsole.Identity;
using AdminConsole.Services;
using AdminConsole.Services.Mail;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminConsole.Pages.Organization;

public class SettingsModel : PageModel
{
    private readonly DataService _dataService;
    private readonly SharedBillingService _billingService;
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly IMailService _mailService;
    private readonly ISystemClock _systemClock;
    private readonly ILogger<SettingsModel> _logger;

    [BindProperty]
    public Models.Organization Organization { get; set; }

    public SettingsModel(
        DataService dataService,
        SharedBillingService billingService,
        SignInManager<ConsoleAdmin> signInManager,
        IMailService mailService,
        ISystemClock systemClock,
        ILogger<SettingsModel> logger)
    {
        _dataService = dataService;
        _billingService = billingService;
        _signInManager = signInManager;
        _mailService = mailService;
        _systemClock = systemClock;
        _logger = logger;
    }

    public async Task OnGet()
    {
        var organization = await _dataService.GetOrganizationWithData();
        Id = organization.Id;
        Name = organization.Name;
        ApplicationsCount = organization.Applications.Count;
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var username = User.Identity?.Name ?? throw new InvalidOperationException();
        if (!string.Equals(Name, NameConfirmation, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Name confirmation does not match.");
            return Page();
        }

        var organization = await _dataService.GetOrganizationWithData();
        var emails = organization.Admins.Select(x => x.Email).ToList();
        await _mailService.SendOrganizationDeletedAsync(organization.Name, emails, username, _systemClock.UtcNow.UtcDateTime);

        if (organization.HasSubscription)
        {
            var isSubscriptionDeleted = await _billingService.CancelSubscription(organization.BillingSubscriptionId!);
            if (!isSubscriptionDeleted)
            {
                _logger.LogError(
                    "Organization {orgId} tried to cancel subscription {subscriptionId}, but failed.",
                    organization.Name,
                    organization.BillingSubscriptionId);
                throw new Exception("Failed to cancel subscription.");
            }
        }

        var isDeleted = await _dataService.DeleteOrganizationAsync(Id);
        if (isDeleted)
        {
            await _signInManager.SignOutAsync();
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Organization Id
    /// </summary>
    [BindProperty]
    public int Id { get; set; }

    /// <summary>
    /// Organization Name
    /// </summary>
    [BindProperty]
    public string Name { get; set; }

    /// <summary>
    /// The organization's name which is confirmed by the end user to allow the organization to be deleted.
    /// </summary>
    [BindProperty]
    public string NameConfirmation { get; set; } = string.Empty;

    /// <summary>
    /// Whether the organization can be deleted.
    /// </summary>
    public bool CanDelete => ApplicationsCount == 0;

    /// <summary>
    /// The amount of active applications belonging to the organization.
    /// </summary>
    [BindProperty]
    public int ApplicationsCount { get; set; }
}