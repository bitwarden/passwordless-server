using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.Organization;

public class Admins : PageModel
{
    private readonly IDataService _dataService;
    private readonly IInvitationService _invitationService;
    private readonly UserManager<ConsoleAdmin> _userManager;
    private readonly SignInManager<ConsoleAdmin> _signinManager;
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IEventLogger _eventLogger;
    private readonly ISystemClock _systemClock;

    public Admins(
        IDataService dataService,
        IInvitationService invitationService,
        UserManager<ConsoleAdmin> userManager,
        SignInManager<ConsoleAdmin> signinManager,
        IPasswordlessClient passwordlessClient,
        IEventLogger eventLogger,
        ISystemClock systemClock)
    {
        _dataService = dataService;
        _invitationService = invitationService;
        _userManager = userManager;
        _signinManager = signinManager;
        _passwordlessClient = passwordlessClient;
        _eventLogger = eventLogger;
        _systemClock = systemClock;
    }

    public List<ConsoleAdmin> ConsoleAdmins { get; set; }

    public List<Invite> Invites { get; set; }
    public InviteForm Form { get; set; }
    public bool CanInviteAdmin { get; set; }

    public async Task<IActionResult> OnGet()
    {
        ConsoleAdmins = await _dataService.GetConsoleAdminsAsync();
        Invites = await _invitationService.GetInvitesAsync(User.GetOrgId().Value);
        CanInviteAdmin = await _dataService.CanInviteAdminAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDelete(string userId)
    {
        var users = await _dataService.GetConsoleAdminsAsync();
        if (users is not { Count: > 1 })
        {
            ModelState.AddModelError("error", "At least one admin is required in an organization.");
            return await OnGet();
        }

        var user = users.FirstOrDefault(u => u.Id == userId);
        if (user is null)
        {
            ModelState.AddModelError("error", "User not found.");
            return await OnGet();
        }

        // Delete Credentials + aliases
        await _passwordlessClient.DeleteUserAsync(user.Id);
        // Delete from admin consoles
        await _userManager.DeleteAsync(user);

        var performedBy = users.FirstOrDefault(x => x.Email == User.GetEmail());
        if (performedBy is not null)
            _eventLogger.LogDeleteAdminEvent(performedBy, user, _systemClock.UtcNow.UtcDateTime);

        // if user is self
        if (user.Email == User.GetEmail())
        {
            TempData.TryAdd("Message", "You have been removed from the organization.");
            await _signinManager.SignOutAsync();
            return RedirectToPage(null);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostInvite(InviteForm form)
    {
        CanInviteAdmin = await _dataService.CanInviteAdminAsync();
        if (CanInviteAdmin is false)
        {
            ModelState.AddModelError("error", "You need to upgrade to a paid organization to invite more admins.");
            return await OnGet();
        }

        if (!ModelState.IsValid)
        {
            // todo: Is there a pattern where we don't need to repeat this?
            return await OnGet();
        }

        Models.Organization org = await _dataService.GetOrganizationAsync();
        var orgId = org.Id;
        var orgName = org.Name;
        ConsoleAdmin user = await _dataService.GetUserAsync();
        var userEmail = user.Email;
        var userName = user.Name;

        await _invitationService.SendInviteAsync(form.Email, orgId, orgName, userEmail, userName);

        _eventLogger.LogInviteAdminEvent(user, form.Email, _systemClock.UtcNow.UtcDateTime);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancel(string hashedCode)
    {
        await _invitationService.CancelInviteAsync(hashedCode);

        var performedBy = await _dataService.GetUserAsync();

        var invitationCancelled = Invites.FirstOrDefault(x => x.HashedCode == hashedCode);
        if (invitationCancelled is not null)
        {
            _eventLogger.LogCancelAdminInviteEvent(
                performedBy,
                invitationCancelled.ToEmail,
                _systemClock.UtcNow.UtcDateTime
            );
        }

        return RedirectToPage();
    }
}

public class InviteForm
{
    [Required, EmailAddress, MaxLength(50)]
    public string Email { get; set; }
}