using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.MagicLinks;

namespace Passwordless.AdminConsole.Pages.Account;

public class LoginModel : PageModel
{
    private readonly MagicLinkSignInManager<ConsoleAdmin> _signInManager;
    private readonly IDataService _dataService;

    public LoginStatus? Status { get; set; }

    public LoginModel(
        MagicLinkSignInManager<ConsoleAdmin> signInManager,
        IDataService dataService)
    {
        _signInManager = signInManager;
        _dataService = dataService;
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
            return RedirectToPage();
        }

        var organization = await _dataService.GetOrganizationAsync(user.OrganizationId);
        if (organization == null)
        {
            return RedirectToPage();
        }
        if (!user.Organization.IsMagicLinksEnabled)
        {
            TempData["Status"] = LoginStatus.MagicLinkDisabled;
        }

        try
        {
            await _signInManager.SendEmailForSignInAsync(email, returnUrl);
        }
        catch (Exception)
        {
            // Ignore any exceptions and just say we sent email to avoid account enumeration
        }

        TempData["Status"] = LoginStatus.EmailSent;

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