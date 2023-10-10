using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.Organization;

public class OverviewModel : PageModel
{
    private readonly DataService _dataService;
    private readonly UserManager<ConsoleAdmin> userManager;

    public OverviewModel(DataService dataService, UserManager<ConsoleAdmin> userManager)
    {
        _dataService = dataService;
        this.userManager = userManager;
    }

    public bool CanCreateApplication { get; set; }
    public Models.Organization Org { get; set; }
    public async Task<IActionResult> OnGet()
    {
        CanCreateApplication = await _dataService.AllowedToCreateApplication();
        var orgData = await _dataService.GetOrganizationWithData();

        if (orgData != null)
        {
            Org = orgData;
            return Page();
        }
        else
        {
            return RedirectToPage("/Error", new { message = "Organization not found" });
        }
    }

    public string GetApplicationUrl(Models.Application application)
    {
        string url = application.DeleteAt.HasValue
            ? "/app/Settings/Settings"
            : "/app/credentials/list";
        return url;
    }
}