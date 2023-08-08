using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages.Settings;

public class SettingsModel : PageModel
{
    private const string Unknown = "unknown";
    private readonly ILogger<SettingsModel> _logger;
    private readonly DataService _dataService;
    private readonly ICurrentContext _currentContext;
    private readonly PasswordlessManagementClient _client;

    public Models.Organization Organization { get; set; }
    public string ApplicationId { get; set; }
    public bool PendingDelete { get; set; }
    public DateTime? DeleteAt { get; set; }

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
        try
        {
            await GetDeletedState();
        }
        catch (Exception ex)
        {
            _logger.LogError("failed to get deleted stuff.");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userName = User.Identity?.Name ?? Unknown;
        var applicationId = _currentContext.AppId ?? Unknown;

        if (userName == Unknown || applicationId == Unknown)
        {
            _logger.LogError("Failed to delete application with name: {appName} and by user: {username}.", applicationId, userName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected happened. Please try again later." });
        }

        await _client.MarkDeleteApplication(new MarkDeleteApplicationRequest(applicationId, userName));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        var applicationId = _currentContext.AppId ?? Unknown;

        try
        {
            _ = await _client.CancelApplicationDeletion(applicationId);
            return RedirectToPage();
        }
        catch (Exception)
        {
            _logger.LogError("Failed to cancel application deletion for application: {appId}", applicationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }

    private async Task GetDeletedState()
    {
        var application = await _client.GetApplicationInformation(ApplicationId);

        PendingDelete = application.DeleteAt != null;
        DeleteAt = GetDeleteAt(application);
    }

    private static DateTime? GetDeleteAt(ApplicationInformationResponse? application) =>
        application is { DeleteAt: not null }
            ? application.DeleteAt.Value.ToLocalTime()
            : null;
}