using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Pages.Account;

public class StepUp(SignInManager<ConsoleAdmin> signInManager, IHttpContextAccessor accessor) : PageModel
{

    public async Task<IActionResult> OnGet(string purpose, string returnUrl)
    {
        Admin = (await signInManager.UserManager.GetUserAsync(User))!;
        RequestedPurpose = purpose;
        CspNonce = accessor.HttpContext!.Items["csp-nonce"]!.ToString()!;

        if (string.IsNullOrWhiteSpace(Request.Query["returnUrl"].ToString()))
        {
            return Redirect("/");
        }

        return Page();
    }
    public ConsoleAdmin Admin { get; set; }
    [BindProperty] public string RequestedPurpose { get; set; } = string.Empty;
    [BindProperty] public string ReturnUrl { get; set; } = string.Empty;
    [BindProperty] public string StepUpVerifyToken { get; set; } = string.Empty;

    [BindProperty] public string CspNonce { get; set; }
}