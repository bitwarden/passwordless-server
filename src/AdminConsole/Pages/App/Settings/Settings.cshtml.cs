using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole;

namespace AdminConsole.Pages.Settings;

public class SettingsModel : PageModel
{
    private readonly ILogger<SettingsModel> _logger;
    private readonly DataService _dataService;
    private readonly ICurrentContext _currentContext;

    public Models.Organization Organization { get; set; }
    public string ApplicationId { get; set; }

    public SettingsModel(ILogger<SettingsModel> logger, DataService dataService, ICurrentContext currentContext)
    {
        _logger = logger;
        _dataService = dataService;
        _currentContext = currentContext;
    }

    public async Task OnGet()
    {
        Organization = await _dataService.GetOrganization();
        ApplicationId = _currentContext.AppId ?? String.Empty;
    }
}