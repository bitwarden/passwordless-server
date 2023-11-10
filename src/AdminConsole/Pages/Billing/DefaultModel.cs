using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Passwordless.AdminConsole.Pages.Billing;

public class DefaultModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Billing/Manage");
    }
}