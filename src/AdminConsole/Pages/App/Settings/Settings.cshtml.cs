using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class SettingsModel : PageModel
{
    private const string Unknown = "unknown";
    private readonly ILogger<SettingsModel> _logger;
    private readonly IDataService _dataService;
    private readonly ICurrentContext _currentContext;
    private readonly IApplicationService _appService;

    public Models.Organization Organization { get; set; }
    public string ApplicationId { get; set; }
    public bool PendingDelete { get; set; }
    public DateTime? DeleteAt { get; set; }

    public SettingsModel(ILogger<SettingsModel> logger, IDataService dataService, ICurrentContext currentContext, IApplicationService appService)
    {
        _logger = logger;
        _dataService = dataService;
        _currentContext = currentContext;
        _appService = appService;
    }

    public async Task OnGet()
    {
        Organization = await _dataService.GetOrganizationWithDataAsync();
        ApplicationId = _currentContext.AppId ?? String.Empty;

        var application = Organization.Applications.FirstOrDefault(x => x.Id == ApplicationId);

        PendingDelete = application?.DeleteAt.HasValue ?? false;
        DeleteAt = application?.DeleteAt;
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

        var response = await _appService.MarkApplicationForDeletionAsync(applicationId, userName);

        return response.IsDeleted ? RedirectToPage("/Organization/Overview") : RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        var applicationId = _currentContext.AppId ?? Unknown;

        try
        {
            await _appService.CancelDeletionForApplicationAsync(applicationId);

            return RedirectToPage();
        }
        catch (Exception)
        {
            _logger.LogError("Failed to cancel application deletion for application: {appId}", applicationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }
}