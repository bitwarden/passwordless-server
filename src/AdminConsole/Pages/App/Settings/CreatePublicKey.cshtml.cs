using Microsoft.AspNetCore.Mvc;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.RoutingHelpers;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Constants;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class CreatePublicKeyModel : BaseExtendedPageModel
{
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ICurrentContext _currentContext;
    private readonly IEventLogger _eventLogger;
    private readonly ILogger<SettingsModel> _logger;

    public CreatePublicKeyModel(
        IPasswordlessManagementClient managementClient,
        ICurrentContext currentContext,
        IEventLogger eventLogger,
        ILogger<SettingsModel> logger)
    {
        _managementClient = managementClient;
        _currentContext = currentContext;
        _eventLogger = eventLogger;
        _logger = logger;

        string[] scopes = Enum.GetValues(typeof(PublicKeyScopes)).Cast<PublicKeyScopes>().Select(x => x.GetValue()).ToArray();
        Model = new(scopes);
    }

    public CreateApiKeyModel Model { get; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var selectedScopesValue = Request.Form[Model.SelectedScopesField].ToString();
        if (string.IsNullOrEmpty(selectedScopesValue))
        {
            ModelState.AddModelError("error", "Please select at least one scope.");
            return Page();
        }

        var selectedScopes = selectedScopesValue
            .Split(',')
            .Select(x => x.AsPublicKeyScope())
            .ToHashSet();

        try
        {
            var request = new CreatePublicKeyRequest(selectedScopes);
            await _managementClient.CreateApiKeyAsync(_currentContext.AppId!, request);

            var eventDto = new OrganizationEventDto(Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyCreated,
                $"Created public key for application {_currentContext.AppId}.",
                Severity.Informational,
                _currentContext.AppId!,
                _currentContext.OrgId!.Value,
                DateTime.UtcNow);
            _eventLogger.LogEvent(eventDto);

            return RedirectToApplicationPage("/App/Settings/Settings", new ApplicationPageRoutingContext(_currentContext.AppId!));
        }
        catch (Exception)
        {
            _logger.LogError("Failed to create public key for application: {appId}", _currentContext.AppId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }
}