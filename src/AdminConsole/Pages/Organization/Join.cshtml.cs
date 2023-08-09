using AdminConsole.Identity;
using AdminConsole.Services;
using AdminConsole.Services.Mail;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Helpers;

namespace AdminConsole.Pages.Organization;

public class Join : PageModel
{
    private readonly InvitationService _invitationService;
    private readonly MagicLinkSignInManager<ConsoleAdmin> _magicLinkSignInManager;
    private readonly IMailService _mailService;
    private readonly IValidator<JoinForm> _validator;
    private readonly UserManager<ConsoleAdmin> _userManager;

    public Join(InvitationService invitationService,
        UserManager<ConsoleAdmin> userManager, MagicLinkSignInManager<ConsoleAdmin> magicLinkSignInManager,
        IMailService mailService, IValidator<JoinForm> validator)
    {
        _invitationService = invitationService;
        _userManager = userManager;
        _magicLinkSignInManager = magicLinkSignInManager;
        _mailService = mailService;
        _validator = validator;
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
        var ok = await _invitationService.ConsumeInvite(invite);

        if (!ok)
        {
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

        public string Email { get; set; }

        public bool AcceptsTermsAndPrivacy { get; set; }

        public string Name { get; set; }
    }

    public class JoinFormValidator : AbstractValidator<JoinForm>
    {
        public JoinFormValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(50);
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(50);
            RuleFor(x => x.AcceptsTermsAndPrivacy).Must(x => x)
                .WithMessage("You must accept the terms and privacy policy to continue.");
        }
    }
}