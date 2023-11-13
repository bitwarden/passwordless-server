using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.MagicLinks;

namespace Passwordless.AdminConsole.Pages.Account;

public class Magic : PageModel
{
    private readonly MagicLinkSignInManager<ConsoleAdmin> _signInManager;
    public bool Success { get; set; }
    public Magic(MagicLinkSignInManager<ConsoleAdmin> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet(string token, string email, string? returnUrl)
    {

        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToPage("/Organization/Overview");
        }

        if (string.IsNullOrEmpty(token))
        {
            Success = false;
            return Page();
        }

        var res = await _signInManager.PasswordlessSignInAsync(email, token, true);

        if (res.Succeeded)
        {
            returnUrl ??= Url.Page("/Organization/Overview");
            Success = true;
            return LocalRedirect(returnUrl);
        }
        Success = false;

        return Page();
    }
}