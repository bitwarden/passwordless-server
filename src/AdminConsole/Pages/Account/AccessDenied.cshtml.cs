using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Pages.Account;

public class AccessDenied : PageModel
{
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly StepUpPurpose _stepUpPurpose;
    public string ReturnUrl { get; set; }

    public AccessDenied(SignInManager<ConsoleAdmin> signInManager, StepUpPurpose stepUpPurpose)
    {
        _signInManager = signInManager;
        _stepUpPurpose = stepUpPurpose;
    }

    public async Task<IActionResult> OnGet()
    {
        var url = Request.Query["ReturnUrl"];

        if (User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(_stepUpPurpose.Purpose))
        {
            return RedirectToPage("/Account/StepUp", new { returnUrl = url, purpose = _stepUpPurpose.Purpose });
        }

        var user = await _signInManager.UserManager.GetUserAsync(User);

        await _signInManager.RefreshSignInAsync(user!);

        if (Url.IsLocalUrl(url))
        {
            ReturnUrl = url;
        }

        return Page();
    }
}