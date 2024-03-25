using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages;

public class IndexModel(ILogger<IndexModel> logger, ISetupService setupService) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        if (await setupService.HasSetupCompletedAsync())
        {
            return RedirectToPage(HttpContext.User.Identity!.IsAuthenticated ? "/account/login" : "/Organization/overview");
        }

        logger.LogInformation("Database has not been initialized. Starting initialization process.");

        return Redirect("/initialize");
    }
}