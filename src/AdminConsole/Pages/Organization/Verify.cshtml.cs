using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.MagicLinks;

namespace Passwordless.AdminConsole.Pages.Organization;

public class Verify : PageModel
{
    private readonly MagicLinkSignInManager<ConsoleAdmin> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Verify(MagicLinkSignInManager<ConsoleAdmin> signInManager, IHttpContextAccessor httpContextAccessor)
    {
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult OnGet()
    {
        return _httpContextAccessor.HttpContext != null && _signInManager.IsSignedIn(_httpContextAccessor.HttpContext.User)
            ? RedirectToPage("/Account/useronboarding")
            : Page();
    }
}