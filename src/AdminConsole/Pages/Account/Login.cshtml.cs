using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.MagicLinks;

namespace Passwordless.AdminConsole.Pages.Account;

public class LoginModel : PageModel
{
    private readonly MagicLinkSignInManager<ConsoleAdmin> signInManager;
    public bool EmailSent { get; set; }

    public LoginModel(MagicLinkSignInManager<ConsoleAdmin> signInManager)
    {
        this.signInManager = signInManager;
    }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (HttpContext.User.Identity is { IsAuthenticated: true })
        {
            returnUrl ??= Url.Page("/Organization/Overview");
            return LocalRedirect(returnUrl);
        }

        EmailSent = TempData["EmailSent"] is not null;

        return Page();
    }

    public async Task<IActionResult> OnPost(string email, string? returnUrl = null)
    {
        returnUrl = Url.Page("/Organization/Overview");

        try
        {
            await signInManager.SendEmailForSignInAsync(email, returnUrl);
        }
        catch (Exception)
        {
            // Ignore any exceptions and just say we sent email to avoid account enumeration
        }

        TempData["EmailSent"] = true;

        return RedirectToPage();
    }

    public IActionResult OnPostSignUp()
    {
        return RedirectToPage("/Organization/Create");
    }
}