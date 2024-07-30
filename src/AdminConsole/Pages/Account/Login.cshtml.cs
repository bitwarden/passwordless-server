using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.MagicLinks;
using Passwordless.AdminConsole.Services.Mail;

namespace Passwordless.AdminConsole.Pages.Account;

public class LoginModel : PageModel
{
    private readonly MagicLinkSignInManager<ConsoleAdmin> _signInManager;
    private readonly IDataService _dataService;
    private readonly IMailService _mailService;

    public LoginStatus? Status { get; set; }

    public LoginModel(
        MagicLinkSignInManager<ConsoleAdmin> signInManager,
        IDataService dataService,
        IMailService mailService)
    {
        _signInManager = signInManager;
        _dataService = dataService;
        _mailService = mailService;
    }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (HttpContext.User.Identity is { IsAuthenticated: true })
        {
            returnUrl ??= Url.Page("/Organization/Overview");
            return LocalRedirect(returnUrl);
        }

        int? status = (int?)TempData["Status"];
        Status = status.HasValue ? (LoginStatus)status.Value : null;

        return Page();
    }

    public async Task<IActionResult> OnPost(string email, string? returnUrl = null)
    {
        returnUrl = Url.Page("/Organization/Overview");

        var user = await _signInManager.UserManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Just say we sent email to avoid account enumeration
            TempData["Status"] = LoginStatus.EmailSent;
            return RedirectToPage();
        }

        var organization = await _dataService.GetOrganizationAsync(user.OrganizationId);
        if (organization == null)
        {
            throw new InvalidOperationException("User does not belong to an organization.");
        }
        if (!organization.IsMagicLinksEnabled)
        {
            TempData["Status"] = LoginStatus.MagicLinkDisabled;
            await _mailService.SendMagicLinksDisabledAsync(organization.Name, email);
            return RedirectToPage();
        }

        TempData["Status"] = LoginStatus.EmailSent;
        await _signInManager.SendEmailForSignInAsync(email, returnUrl);

        return RedirectToPage();
    }

    public IActionResult OnPostSignUp()
    {
        return RedirectToPage("/Organization/Create");
    }

    public enum LoginStatus
    {
        EmailSent,
        MagicLinkDisabled
    }
}