using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.Mail;

namespace Passwordless.AdminConsole.Pages.Organization;

public class Join : PageModel
{
    private readonly IInvitationService _invitationService;
    private readonly MagicLinkSignInManager<ConsoleAdmin> _magicLinkSignInManager;
    private readonly IMailService _mailService;
    private readonly IEventLogger _eventLogger;
    private readonly TimeProvider _timeProvider;
    private readonly UserManager<ConsoleAdmin> _userManager;

    public Join(IInvitationService invitationService,
        UserManager<ConsoleAdmin> userManager, MagicLinkSignInManager<ConsoleAdmin> magicLinkSignInManager,
        IMailService mailService,
        IEventLogger eventLogger,
        TimeProvider timeProvider)
    {
        _invitationService = invitationService;
        _userManager = userManager;
        _magicLinkSignInManager = magicLinkSignInManager;
        _mailService = mailService;
        _eventLogger = eventLogger;
        _timeProvider = timeProvider;
    }

    public Invite Invite { get; set; }
    public JoinForm Form { get; set; }

    public async Task<IActionResult> OnGet(string code)
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToPage("JoinBusy", new { code = code });
        }

        try
        {
            Invite = await _invitationService.GetInviteFromRawCodeAsync(code);
        }
        catch (Exception)
        {
            Invite = null;
        }

        if (Invite == null)
        {
            ModelState.AddModelError("bad-invite", "Invite is invalid or expired");
            return Page();
        }
        // todo: We could add a check if the email is busy here and if so show a message.

        Form = new JoinForm { Code = code, Email = Invite.ToEmail };

        return Page();
    }

    public async Task<IActionResult> OnPost(JoinForm form)
    {
        if (!form.AcceptsTermsAndPrivacy)
        {
            ModelState.AddModelError("AcceptsTermsAndPrivacy", "You must accept the terms and privacy policy to continue.");
        }

        if (!ModelState.IsValid)
        {
            Invite = await _invitationService.GetInviteFromRawCodeAsync(form.Code);
            return Page();
        }

        Invite invite = await _invitationService.GetInviteFromRawCodeAsync(form.Code);
        var ok = await _invitationService.ConsumeInviteAsync(invite);

        if (!ok)
        {
            _eventLogger.LogAdminInvalidInviteUsedEvent(invite, _timeProvider.GetUtcNow().UtcDateTime);
            ModelState.AddModelError("bad-invite", "Invite is invalid or expired");
        }

        ConsoleAdmin? existingUser = await _userManager.FindByEmailAsync(form.Email);

        if (existingUser == null)
        {
            // create account
            var user = new ConsoleAdmin
            {
                UserName = form.Email,
                Email = form.Email,
                OrganizationId = invite.TargetOrgId,
                Name = form.Name
            };

            await _userManager.CreateAsync(user);

            var url = Url.Page("/Account/useronboarding");
            await _magicLinkSignInManager.SendEmailForSignInAsync(user.Email, url);

            _eventLogger.LogAdminAcceptedInviteEvent(invite, user, _timeProvider.GetUtcNow().UtcDateTime);
        }
        else
        {
            await _mailService.SendEmailIsAlreadyInUseAsync(existingUser.Email);
        }

        return RedirectToPage("/Organization/Verify");
        // redirect to account onboarding setup passkey
    }

    public class JoinForm
    {
        public string Code { get; set; }

        [Required, EmailAddress, MaxLength(50)]
        public string Email { get; set; }

        [Required]
        public bool AcceptsTermsAndPrivacy { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
    }
}