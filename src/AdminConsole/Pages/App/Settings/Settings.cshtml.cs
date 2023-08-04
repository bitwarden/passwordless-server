using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.Models.DTOs;

namespace AdminConsole.Pages.Settings;

public class SettingsModel : PageModel
{
    private readonly ILogger<SettingsModel> _logger;
    private readonly DataService _dataService;
    private readonly ICurrentContext _currentContext;
    private readonly PasswordlessManagementClient _client;

    public Models.Organization Organization { get; set; }
    public string ApplicationId { get; set; }

    public SettingsModel(ILogger<SettingsModel> logger, DataService dataService, ICurrentContext currentContext, PasswordlessManagementClient client)
    {
        _logger = logger;
        _dataService = dataService;
        _currentContext = currentContext;
        _client = client;
    }

    public async Task OnGet()
    {
        Organization = await _dataService.GetOrganization();
        ApplicationId = _currentContext.AppId ?? String.Empty;
    }

    public async Task<IActionResult> OnPost()
    {
        const string unknown = "unknown";

        var userName = User.Identity?.Name ?? unknown;
        var applicationId = _currentContext.AppId ?? unknown;

        if (userName == unknown || applicationId == unknown)
        {
            _logger.LogError("Failed to delete application with name: {appName} and by user: {username}.", applicationId, userName);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Something unexpected happened. Please try again later."
            });
        }

        var result = await _client.MarkDeleteApplication(new MarkDeleteApplicationRequest(applicationId, userName));

        return new JsonResult(result);
    }
}