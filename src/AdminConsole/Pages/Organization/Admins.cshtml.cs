using AdminConsole.Helpers;
using AdminConsole.Identity;
using AdminConsole.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Helpers;
using Passwordless.Net;

namespace AdminConsole.Pages.Organization;

public class Admins : PageModel
{
    private readonly DataService _dataService;
    private readonly InvitationService _invitationService;
    private readonly UserManager<ConsoleAdmin> _userManager;
    private readonly SignInManager<ConsoleAdmin> _signinManager;
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IValidator<InviteForm> _validator;

    public Admins(DataService dataService,
        InvitationService invitationService, UserManager<ConsoleAdmin> userManager, SignInManager<ConsoleAdmin> signinManager, IPasswordlessClient passwordlessClient, IValidator<InviteForm> validator)
    {
        _dataService = dataService;
        _invitationService = invitationService;
        _userManager = userManager;
        _signinManager = signinManager;
        _passwordlessClient = passwordlessClient;
        _validator = validator;
    }

    public List<ConsoleAdmin> ConsoleAdmins { get; set; }

    public List<Invite> Invites { get; set; }
    public InviteForm Form { get; set; }
    public bool CanInviteAdmin { get; set; }

    public async Task<IActionResult> OnGet()
    {
        ConsoleAdmins = await _dataService.GetConsoleAdmins();
        Invites = await _invitationService.GetInvites(User.GetOrgId());
        CanInviteAdmin = await _dataService.CanInviteAdmin();

        return Page();
    }

    public async Task<IActionResult> OnPostDelete(string userId)
    {
        var users = await _dataService.GetConsoleAdmins();
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
        CanInviteAdmin = await _dataService.CanInviteAdmin();
        if (CanInviteAdmin is false)
        {
            ModelState.AddModelError("error", "You need to upgrade to a paid organization to invite more admins.");
            return await OnGet();
        }

        var validationResult = await _validator.ValidateAsync(form);
        validationResult.AddToModelState(ModelState, nameof(Form));

        if (!ModelState.IsValid)
        {
            // todo: Is there a pattern where we don't need to repeat this?
            return await OnGet();
        }

        Models.Organization org = await _dataService.GetOrganization();
        var orgId = org.Id;
        var orgName = org.Name;
        ConsoleAdmin user = await _dataService.GetUserAsync();
        var userEmail = user.Email;
        var userName = user.Name;

        await _invitationService.SendInviteAsync(form.Email, orgId, orgName, userEmail, userName);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancel(string hashedCode)
    {
        await _invitationService.CancelInviteAsync(hashedCode);
        return RedirectToPage();
    }
}

public class InviteForm
{
    public string Email { get; set; }
}

public class InviteFormValidator : AbstractValidator<InviteForm>
{
    public InviteFormValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(50);
    }
}