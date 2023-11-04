using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Billing;

public class ChangePlanModel : PageModel
{
    private readonly IDataService _dataService;

    public ChangePlanModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public Application Application { get; private set; }

    public async Task OnGet(string appId)
    {
        var application = await _dataService.GetApplicationAsync(appId);
        if (Application == null) throw new InvalidOperationException("Application not found.");
        Application = application!;
    }
}