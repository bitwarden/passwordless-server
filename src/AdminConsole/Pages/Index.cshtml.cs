using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Db;

namespace Passwordless.AdminConsole.Pages;

public class IndexModel(ILogger<IndexModel> logger, ConsoleDbContext dbContext) : PageModel
{
    public IActionResult OnGet()
    {
        if (dbContext.Database.CanConnect())
        {
            if (HttpContext.User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/account/login");
            }

            return RedirectToPage("/Organization/overview");
        }
        
        logger.LogInformation("Database does not exist. Starting initialization process.");

        return RedirectToPage("/Initialize");
    }
}