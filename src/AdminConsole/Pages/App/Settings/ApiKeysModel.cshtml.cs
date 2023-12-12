using System.Collections.Immutable;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;
using Passwordless.Common.EventLog.Enums;

namespace Passwordless.AdminConsole.Pages.App.Settings;

public class ApiKeysModel
{
    public const string SelectedApiKeyIdField = "SelectedApiKeyId";

    private readonly IPasswordlessManagementClient _managementClient;
    private readonly ICurrentContext _currentContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventLogger _eventLogger;
    private readonly ILogger _logger;

    public ApiKeysModel(
        IPasswordlessManagementClient managementClient,
        ICurrentContext currentContext,
        IHttpContextAccessor httpContextAccessor,
        IEventLogger eventLogger,
        ILogger logger)
    {
        _managementClient = managementClient;
        _currentContext = currentContext;
        _httpContextAccessor = httpContextAccessor;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public string ApplicationId => _currentContext.AppId!;

    public IReadOnlyCollection<ApiKey> ApiKeys { get; private set; }

    public record ApiKey(
        string Id,
        string Value,
        string Type,
        IReadOnlySet<string> Scopes,
        bool IsLocked,
        DateTime? LastLockedAt,
        bool IsActiveManagementKey,
        bool CanDelete)
    {
        public static ApiKey FromDto(ApiKeyResponse dto, ICurrentContext currentContext)
        {
            bool isActiveManagementKey = dto.Type == ApiKeyTypes.Public
                ? currentContext.ApiKey!.EndsWith(dto.Id)
                : currentContext.ApiSecret!.EndsWith(dto.Id);
            return new ApiKey(
                dto.Id,
                dto.ApiKey,
                dto.Type.ToString(),
                dto.Scopes,
                dto.IsLocked,
                dto.LastLockedAt,
                isActiveManagementKey,
                dto.IsLocked);
        }
    }

    public async Task OnInitializeAsync()
    {
        var apiKeys = await _managementClient.GetApiKeysAsync(ApplicationId);
        ApiKeys = apiKeys
            .Select(x => ApiKey.FromDto(x, _currentContext))
            .Where(x => !x.IsActiveManagementKey)
            .ToImmutableList();
    }

    public async Task OnLockAsync()
    {
        var applicationId = _currentContext.AppId ?? throw new ArgumentNullException(nameof(_currentContext.AppId));
        var selectedApiKeyId = _httpContextAccessor.HttpContext!.Request.Form[SelectedApiKeyIdField].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await _managementClient.LockApiKeyAsync(applicationId, selectedApiKeyId);

            var eventDto = new OrganizationEventDto(_httpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyLocked,
                $"Locked API key '{selectedApiKeyId}' for application {applicationId}.",
                Severity.Informational,
                _currentContext.AppId!,
                _currentContext.OrgId!.Value,
                DateTime.UtcNow);
            _eventLogger.LogEvent(eventDto);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to lock api key for application: {appId}", applicationId);
            throw;
        }
    }

    public async Task OnUnlockAsync()
    {
        var applicationId = _currentContext.AppId ?? throw new ArgumentNullException(nameof(_currentContext.AppId));
        var selectedApiKeyId = _httpContextAccessor.HttpContext!.Request.Form["SelectedApiKeyId"].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await _managementClient.UnlockApiKeyAsync(applicationId, selectedApiKeyId);

            var eventDto = new OrganizationEventDto(_httpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyUnlocked,
                $"Unlocked API key '{selectedApiKeyId}' for application {applicationId}.",
                Severity.Informational,
                _currentContext.AppId!,
                _currentContext.OrgId!.Value,
                DateTime.UtcNow);
            _eventLogger.LogEvent(eventDto);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to unlock api key for application: {appId}", applicationId);
            throw;
        }
    }

    public async Task OnDeleteAsync()
    {
        var applicationId = _currentContext.AppId ?? throw new ArgumentNullException(nameof(_currentContext.AppId));
        var selectedApiKeyId = _httpContextAccessor.HttpContext!.Request.Form["SelectedApiKeyId"].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await _managementClient.DeleteApiKeyAsync(applicationId, selectedApiKeyId);

            var eventDto = new OrganizationEventDto(_httpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyDeleted,
                $"Deleted API key '{selectedApiKeyId}' for application {applicationId}.",
                Severity.Informational,
                _currentContext.AppId!,
                _currentContext.OrgId!.Value,
                DateTime.UtcNow);
            _eventLogger.LogEvent(eventDto);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to delete api key for application: {appId}", applicationId);
        }
    }
}