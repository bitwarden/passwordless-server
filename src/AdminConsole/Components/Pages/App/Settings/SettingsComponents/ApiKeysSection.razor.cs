using System.Collections.Immutable;
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
    public const string SelectedApiKeyFormName = "selected-api-key-form";

    public string AppId => CurrentContext.AppId!;

    [SupplyParameterFromForm(FormName = CreateApiKeyFormName)]
    public CreateFormModel? CreateForm { get; set; }

    [SupplyParameterFromForm(FormName = SelectedApiKeyFormName)]
    public SelectedFormModel? SelectedForm { get; set; }

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
        CreateForm ??= new CreateFormModel();
        SelectedForm ??= new SelectedFormModel();

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
                await LockSelectedAsync();
                break;
            case "unlock":
                await UnlockSelectedAsync();
                break;
            case "delete":
                await DeleteSelectedAsync();
                break;
        }
    }

    private async Task LockSelectedAsync()
    {
        try
        {
            await ManagementClient.LockApiKeyAsync(AppId, SelectedForm!.ApiKeyId!);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyLocked,
                $"Locked API key '{SelectedForm.ApiKeyId}' for application {AppId}.",
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

    private async Task UnlockSelectedAsync()
    {
        try
        {
            await ManagementClient.UnlockApiKeyAsync(AppId, SelectedForm!.ApiKeyId!);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyUnlocked,
                $"Unlocked API key '{SelectedForm.ApiKeyId}' for application {AppId}.",
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

    private async Task DeleteSelectedAsync()
    {
        try
        {
            await ManagementClient.DeleteApiKeyAsync(AppId, SelectedForm.ApiKeyId);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyDeleted,
                $"Deleted API key '{SelectedForm!.ApiKeyId}' for application {AppId}.",
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