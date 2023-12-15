using System.Text;
using Microsoft.AspNetCore.Mvc;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Constants;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class CreateSecretKeyModel : BaseExtendedPageModel
{
    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ICurrentContext _currentContext;
    private readonly IEventLogger _eventLogger;
    private readonly ILogger<SettingsModel> _logger;

    public CreateSecretKeyModel(
        IPasswordlessManagementClient managementClient,
        ICurrentContext currentContext,
        IEventLogger eventLogger,
        ILogger<SettingsModel> logger
        )
    {
        _managementClient = managementClient;
        _currentContext = currentContext;
        _eventLogger = eventLogger;
        _logger = logger;

        string[] secretScopes = Enum.GetValues(typeof(SecretKeyScopes)).Cast<SecretKeyScopes>().Select(x => x.GetValue()).ToArray();
        Model = new(secretScopes);
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
            .Select(x => x.AsSecretKeyScope())
            .ToHashSet();

        try
        {
            var request = new CreateSecretKeyRequest(selectedScopes);
            var apiKey = await _managementClient.CreateApiKeyAsync(_currentContext.AppId!, request);

            var eventDto = new OrganizationEventDto(
                Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyCreated,
                $"Created secret key for application {_currentContext.AppId}.",
                Severity.Informational,
                _currentContext.AppId!,
                _currentContext.OrgId!.Value,
                DateTime.UtcNow);
            _eventLogger.LogEvent(eventDto);

            var encodedApiKey = Base64Url.Encode(Encoding.UTF8.GetBytes(apiKey.ApiKey));
            return RedirectToPage(
                "/App/Settings/SecretKeyCreated",
                new { App = _currentContext.AppId, EncodedApiKey = encodedApiKey });
        }
        catch (Exception)
        {
            _logger.LogError("Failed to create secret key for application: {appId}", _currentContext.AppId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }
}