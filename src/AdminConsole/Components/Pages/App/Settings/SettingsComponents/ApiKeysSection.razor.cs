using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Components.Shared.Modals;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;

public partial class ApiKeysSection : ComponentBase
{
    public const string CreateApiKeyFormName = "create-api-key-form";
    public const string SelectedApiKeyFormName = "selected-api-key-form";
    public const string ConfirmedSelectedApiKeyFormName = "confirmed-selected-api-key-form";

    public string AppId => CurrentContext.AppId!;

    [SupplyParameterFromForm(FormName = CreateApiKeyFormName)]
    public CreateFormModel CreateForm { get; set; } = new();

    [SupplyParameterFromForm(FormName = SelectedApiKeyFormName)]
    public SelectedFormModel SelectedForm { get; set; } = new();

    [SupplyParameterFromForm(FormName = ConfirmedSelectedApiKeyFormName)]
    public SelectedFormModel ConfirmedSelectedForm { get; set; } = new();

    public IReadOnlyCollection<ApiKey>? ApiKeys { get; private set; }

    public SimpleAlert.SimpleAlertModel? Modal { get; set; }

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
        // If we've posted a form, we need to add backwards compatibility for Razor Pages. Bind it to the model, and trigger the form submission handler.
        if (HttpContextAccessor.IsRazorPages() && HttpContextAccessor.HttpContext!.Request.HasFormContentType)
        {
            var request = HttpContextAccessor.HttpContext!.Request;
            switch (request.Form["_handler"])
            {
                case CreateApiKeyFormName:
                    CreateForm.Type = request.Form["CreateForm.Type"].ToString();
                    OnCreateFormSubmitted();
                    break;
                case SelectedApiKeyFormName:
                    SelectedForm.ApiKeyId = request.Form["SelectedForm.ApiKeyId"].ToString();
                    SelectedForm.Action = request.Form["SelectedForm.Action"].ToString();
                    await OnSelectedFormSubmitted();
                    break;
                case ConfirmedSelectedApiKeyFormName:
                    ConfirmedSelectedForm.ApiKeyId = request.Form["ConfirmedSelectedForm.ApiKeyId"].ToString();
                    ConfirmedSelectedForm.Action = request.Form["ConfirmedSelectedForm.Action"].ToString();
                    await OnSelectedFormConfirmed();
                    break;
            }
        }

        var apiKeys = await ManagementClient.GetApiKeysAsync(AppId);
        ApiKeys = apiKeys
            .Select(x => ApiKey.FromDto(x, CurrentContext))
            .Where(x => !x.IsActiveManagementKey)
            .ToImmutableList();
    }

    private void OnCreateFormSubmitted()
    {
        switch (CreateForm!.Type)
        {
            case "public":
                NavigationManager.NavigateTo($"app/{AppId}/settings/create-public-key");
                break;
            case "secret":
                NavigationManager.NavigateTo($"app/{AppId}/settings/create-secret-key");
                break;
        }
    }

    private async Task OnSelectedFormSubmitted()
    {
        if (string.IsNullOrEmpty(SelectedForm!.ApiKeyId))
        {
            throw new ArgumentNullException(nameof(SelectedForm.ApiKeyId));
        }
        switch (SelectedForm!.Action)
        {
            case "lock":
                Modal = new SimpleAlert.SimpleAlertModel
                {
                    Id = "selected-api-key-modal",
                    Title = "Lock API Key",
                    Description = "Are you sure you want to lock this API key? Are you sure you want to lock this API key? Are you sure you want to lock this API key?",
                    ConfirmButtonId = "confirm-lock-api-key-btn",
                    ConfirmText = "Lock",
                    CancelButtonId = "cancel-lock-api-key-btn",
                    IsHidden = false
                };
                break;
            case "unlock":
                Modal = new SimpleAlert.SimpleAlertModel
                {
                    Id = "selected-api-key-modal",
                    Title = "Unlock API Key",
                    Description = "Are you sure you want to unlock this API key?",
                    ConfirmButtonId = "confirm-unlock-api-key-btn",
                    ConfirmText = "Unlock",
                    CancelButtonId = "cancel-unlock-api-key-btn",
                    IsHidden = false
                };
                break;
            case "delete":
                Modal = new SimpleAlert.SimpleAlertModel
                {
                    Id = "selected-api-key-modal",
                    Title = "Delete API Key",
                    Description = "Are you sure you want to permanently delete this API key?",
                    ConfirmButtonId = "confirm-delete-api-key-btn",
                    ConfirmText = "Delete",
                    CancelButtonId = "cancel-delete-api-key-btn",
                    IsHidden = false
                };
                break;
        }
    }

    private async Task OnSelectedFormConfirmed()
    {
        if (string.IsNullOrEmpty(ConfirmedSelectedForm.ApiKeyId))
        {
            throw new ArgumentNullException(nameof(ConfirmedSelectedForm.ApiKeyId));
        }
        switch (ConfirmedSelectedForm.Action)
        {
            case "lock":
                await LockSelectedAsync(ConfirmedSelectedForm.ApiKeyId);
                break;
            case "unlock":
                await UnlockSelectedAsync(ConfirmedSelectedForm.ApiKeyId);
                break;
            case "delete":
                await DeleteSelectedAsync(ConfirmedSelectedForm.ApiKeyId);
                break;
        }
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
        public string? ApiKeyId { get; set; }
        public string? Action { get; set; }
    }
}