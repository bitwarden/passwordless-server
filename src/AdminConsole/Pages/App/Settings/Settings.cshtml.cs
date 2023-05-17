using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminConsole.Pages.Settings;

public class SettingsModel : PageModel
{
    private readonly ILogger<SettingsModel> _logger;
    private readonly DataService _dataService;

    public Models.Organization Organization { get; set; }

    public SettingsModel(ILogger<SettingsModel> logger, DataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    public async Task OnGet()
    {
        Organization = await _dataService.GetOrganization();
    }
}