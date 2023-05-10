using AdminConsole.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminConsole.Pages.Organization;

public class Verify : PageModel
{
    private readonly MagicLinkSignInManager<ConsoleAdmin> _signInManager;

    public Verify(MagicLinkSignInManager<ConsoleAdmin> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet(string token, string email)
    {
        return Page();
    }
}