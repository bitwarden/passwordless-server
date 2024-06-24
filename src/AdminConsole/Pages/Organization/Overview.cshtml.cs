using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.Organization;

public class OverviewModel : PageModel
{
    private readonly IDataService _dataService;

    public OverviewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public bool CanCreateApplication { get; set; }
    public Models.Organization Org { get; set; }
    public async Task<IActionResult> OnGet()
    {
        CanCreateApplication = await _dataService.AllowedToCreateApplicationAsync();
        var orgData = await _dataService.GetOrganizationWithDataAsync();

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
            ? $"/app/{application.Id}/Settings"
            : $"/app/{application.Id}/credentials/list";
        return url;
    }
}