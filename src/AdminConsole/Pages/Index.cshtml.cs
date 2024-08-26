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
            if (HttpContext.User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/account/login");
            }

            return Redirect("/organization/overview");
        }

        logger.LogInformation("Database has not been initialized. Starting initialization process.");

        return Redirect("/Initialize");
    }
}