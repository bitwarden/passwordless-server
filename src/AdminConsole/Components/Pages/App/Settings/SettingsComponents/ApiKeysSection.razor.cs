using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;

public partial class ApiKeysSection : ComponentBase
{
    public const string CreateApiKeyFormName = "create-api-key-form";
    public const string ConfirmedSelectedApiKeyFormName = "confirmed-selected-api-key-form";

    public string AppId => CurrentContext.AppId!;

    [SupplyParameterFromForm(FormName = CreateApiKeyFormName)]
    public CreateFormModel CreateForm { get; set; } = new();

    [SupplyParameterFromForm(FormName = ConfirmedSelectedApiKeyFormName)]
    public SelectedFormModel ConfirmedSelectedForm { get; set; } = new();

    public IReadOnlyCollection<ApiKey>? ApiKeys { get; private set; }

    public record ApiKey(
        string Id,
        DateTime CreatedAt,
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
                dto.CreatedAt,
                dto.ApiKey,
                dto.Type.ToString(),
                dto.Scopes,
                dto.IsLocked,
                dto.LastLockedAt,
                isActiveManagementKey,
                dto.IsLocked);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (CurrentContext.IsPendingDelete) return;

        var apiKeys = await ManagementClient.GetApiKeysAsync(AppId);
        ApiKeys = apiKeys
            .Select(x => ApiKey.FromDto(x, CurrentContext))
            .Where(x => !x.IsActiveManagementKey)
            .ToImmutableList();
    }

    private void OnCreateFormSubmitted()
    {
        switch (CreateForm.Type)
        {
            case "public":
                NavigationManager.NavigateTo($"app/{AppId}/settings/create-public-key");
                break;
            case "secret":
                NavigationManager.NavigateTo($"app/{AppId}/settings/create-secret-key");
                break;
        }
    }

    private async Task OnSelectedFormConfirmed()
    {
        if (ConfirmedSelectedForm.DeleteAction != null)
        {
            await DeleteSelectedAsync(ConfirmedSelectedForm.DeleteAction!);
        }
        else if (ConfirmedSelectedForm.LockAction != null)
        {
            await LockSelectedAsync(ConfirmedSelectedForm.LockAction!);
        }
        else if (ConfirmedSelectedForm.UnlockAction != null)
        {
            await UnlockSelectedAsync(ConfirmedSelectedForm.UnlockAction!);
        }
        else
        {
            throw new ArgumentNullException(nameof(ConfirmedSelectedForm), "No action selected.");
        }
        NavigationManager.Refresh();
    }

    private async Task LockSelectedAsync(string apiKeyId)
    {
        try
        {
            await ManagementClient.LockApiKeyAsync(AppId, apiKeyId);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyLocked,
                $"Locked API key '{apiKeyId}' for application {AppId}.",
                Severity.Informational,
                AppId,
                CurrentContext.OrgId!.Value,
                DateTime.UtcNow);
            EventLogger.LogEvent(eventDto);
        }
        catch (Exception)
        {
            Logger.LogError("Failed to lock api key for application: {appId}", AppId);
            throw;
        }
    }

    private async Task UnlockSelectedAsync(string apiKeyId)
    {
        try
        {
            await ManagementClient.UnlockApiKeyAsync(AppId, apiKeyId);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyUnlocked,
                $"Unlocked API key '{apiKeyId}' for application {AppId}.",
                Severity.Informational,
                AppId,
                CurrentContext.OrgId!.Value,
                DateTime.UtcNow);
            EventLogger.LogEvent(eventDto);
        }
        catch (Exception)
        {
            Logger.LogError("Failed to unlock api key for application: {appId}", AppId);
            throw;
        }
    }

    private async Task DeleteSelectedAsync(string apiKeyId)
    {
        try
        {
            await ManagementClient.DeleteApiKeyAsync(AppId, apiKeyId);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyDeleted,
                $"Deleted API key '{apiKeyId}' for application {AppId}.",
                Severity.Informational,
                AppId,
                CurrentContext.OrgId!.Value,
                DateTime.UtcNow);
            EventLogger.LogEvent(eventDto);
        }
        catch (Exception)
        {
            Logger.LogError("Failed to delete api key for application: {appId}", AppId);
        }
    }

    public sealed class CreateFormModel
    {
        public string? Type { get; set; }
    }

    public sealed class SelectedFormModel
    {
        [Length(4, 4)]
        public string? DeleteAction { get; set; }

        [Length(4, 4)]
        public string? LockAction { get; set; }

        [Length(4, 4)]
        public string? UnlockAction { get; set; }
    }
}