using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Helpers;

namespace Passwordless.AdminConsole.Pages;

public class IndexModel(ILogger<IndexModel> logger, ConsoleDbContext dbContext) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        if (await dbContext.HaveAnyMigrationsEverBeenAppliedAsync())
        {
            return RedirectToPage(HttpContext.User.Identity!.IsAuthenticated ? "/account/login" : "/Organization/overview");
        }

        logger.LogInformation("Database has not been initialized. Starting initialization process.");

        return Redirect("/initialize");
    }
}