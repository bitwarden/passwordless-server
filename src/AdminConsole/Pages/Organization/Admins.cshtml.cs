using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.RateLimiting;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.Organization;

[EnableRateLimiting(AdminPageRateLimit.PolicyName)]
public class Admins : PageModel
{
    private readonly IDataService _dataService;
    private readonly IInvitationService _invitationService;
    private readonly UserManager<ConsoleAdmin> _userManager;
    private readonly SignInManager<ConsoleAdmin> _signinManager;
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IEventLogger _eventLogger;
    private readonly TimeProvider _timeProvider;

    public Admins(
        IDataService dataService,
        IInvitationService invitationService,
        UserManager<ConsoleAdmin> userManager,
        SignInManager<ConsoleAdmin> signinManager,
        IPasswordlessClient passwordlessClient,
        IEventLogger eventLogger,
        TimeProvider timeProvider)
    {
        _dataService = dataService;
        _invitationService = invitationService;
        _userManager = userManager;
        _signinManager = signinManager;
        _passwordlessClient = passwordlessClient;
        _eventLogger = eventLogger;
        _timeProvider = timeProvider;
    }

    public List<ConsoleAdmin> ConsoleAdmins { get; set; }

    public List<Invite> Invites { get; set; }
    public InviteForm Form { get; set; }
    public bool CanInviteAdmin { get; set; }

    public async Task<IActionResult> OnGet()
    {
        ConsoleAdmins = await _dataService.GetConsoleAdminsAsync();
        Invites = await _invitationService.GetInvitesAsync(User.GetOrgId()!.Value);
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
            _eventLogger.LogDeleteAdminEvent(performedBy, user, _timeProvider.GetUtcNow().UtcDateTime);

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

        var org = await _dataService.GetOrganizationAsync();
        var existingInvites = await _invitationService.GetInvitesAsync(org.Id);

        if (existingInvites.Count >= 10)
        {
            ModelState.AddModelError("error", "You can only have 10 pending invites at a time.");
            return await OnGet();
        }

        if (existingInvites.Any(x => x.ToEmail == form.Email))
        {
            ModelState.AddModelError("error", "There is a pending invite already for this address. Please cancel before resending.");
            return await OnGet();
        }

        if (!ModelState.IsValid)
        {
            // todo: Is there a pattern where we don't need to repeat this?
            return await OnGet();
        }

        ConsoleAdmin user = await _dataService.GetUserAsync();

        await _invitationService.SendInviteAsync(form.Email, org.Id, org.Name, user.Email!, user.Name);

        _eventLogger.LogInviteAdminEvent(user, form.Email, _timeProvider.GetUtcNow().UtcDateTime);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancel(string hashedCode)
    {
        Invites = await _invitationService.GetInvitesAsync(User.GetOrgId().Value);

        var inviteToCancel = Invites.FirstOrDefault(x => x.HashedCode == hashedCode);

        await _invitationService.CancelInviteAsync(hashedCode);

        var performedBy = await _dataService.GetUserAsync();

        if (inviteToCancel is not null)
        {
            _eventLogger.LogCancelAdminInviteEvent(
                performedBy,
                inviteToCancel.ToEmail,
                _timeProvider.GetUtcNow().UtcDateTime
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