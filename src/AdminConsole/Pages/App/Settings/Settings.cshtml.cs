using Microsoft.AspNetCore.Mvc;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services;
using Application = Passwordless.AdminConsole.Models.Application;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class SettingsModel : BaseExtendedPageModel
{
    private const string Unknown = "unknown";
    private readonly ILogger<SettingsModel> _logger;
    private readonly IDataService _dataService;
    private readonly ICurrentContext _currentContext;
    private readonly IApplicationService _appService;

    public SettingsModel(
        ILogger<SettingsModel> logger,
        IDataService dataService,
        ICurrentContext currentContext,
        IApplicationService appService)
    {
        _logger = logger;
        _dataService = dataService;
        _currentContext = currentContext;
        _appService = appService;
    }

    public Models.Organization Organization { get; set; }

    public string ApplicationId { get; private set; }

    public bool PendingDelete { get; set; }

    public DateTime? DeleteAt { get; set; }

    public Application? Application { get; private set; }

    public bool CanDeleteImmediately { get; private set; }

    private async Task InitializeAsync()
    {
        Organization = await _dataService.GetOrganizationWithDataAsync();
        ApplicationId = _currentContext.AppId ?? string.Empty;

        var application = Organization.Applications.FirstOrDefault(x => x.Id == ApplicationId);

        Application = application ?? throw new InvalidOperationException("Application not found.");
        CanDeleteImmediately = await _appService.CanDeleteApplicationImmediatelyAsync(ApplicationId);
    }

    public async Task OnGet()
    {
        await InitializeAsync();

        PendingDelete = Application?.DeleteAt.HasValue ?? false;
        DeleteAt = Application?.DeleteAt;
    }

    /// <summary>
    /// Deletes the application.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userName = User.Identity?.Name ?? Unknown;
        var applicationId = _currentContext.AppId ?? Unknown;

        if (userName == Unknown || applicationId == Unknown)
        {
            _logger.LogError("Failed to delete application with name: {appName} and by user: {username}.", applicationId, userName);
            return RedirectToPage("/Error", new { Message = "Something unexpected happened." });
        }

        try
        {
            var response = await _appService.MarkDeleteApplicationAsync(applicationId, userName);

            return response.IsDeleted ? RedirectToPage("/Organization/Overview") : RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete application: {appName}.", applicationId);
            return RedirectToPage("/Error", new { ex.Message });
        }
    }

    /// <summary>
    /// Cancels the deletion of the application.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostCancelAsync()
    {
        var applicationId = _currentContext.AppId ?? Unknown;

        if (applicationId == Unknown)
        {
            _logger.LogError("Failed to cancel application deletion for application: {appId}", applicationId);
            return RedirectToPage("/Error", new { Message = "Something unexpected happened." });
        }

        try
        {
            await _appService.CancelDeletionForApplicationAsync(applicationId);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to cancel application deletion for application: {appId}", applicationId);
            return RedirectToPage("/Error", new { ex.Message });
        }
    }

    public bool IsAttestationAllowed => _currentContext.Features.AllowAttestation;
}