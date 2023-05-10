using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminConsole.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        if (!HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/account/login");
        }
        else
        {
            return RedirectToPage("/Organization/overview");
        }

        return Page();
    }
}