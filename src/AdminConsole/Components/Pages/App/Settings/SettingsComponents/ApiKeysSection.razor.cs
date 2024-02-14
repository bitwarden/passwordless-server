using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Helpers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;

public partial class ApiKeysSection : ComponentBase
{
    public const string CreateApiKeyFormName = "create-api-key-form";
    
    public const string SelectedApiKeyIdField = "SelectedApiKeyId";

    public string AppId => CurrentContext.AppId!;
    
    [SupplyParameterFromForm(FormName = CreateApiKeyFormName)] public CreateFormModel? CreateForm { get; set; }
    
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
        
        if (HttpContextAccessor.IsRazorPages())
        {
            if (CreateForm.Type == null)
            {
                CreateForm.Type = HttpContextAccessor.HttpContext.Request.Form["CreateForm.Type"].ToString();
                OnCreateFormSubmitted();
            }
        }
        
        var apiKeys = await ManagementClient.GetApiKeysAsync(AppId);
        ApiKeys = apiKeys
            .Select(x => ApiKey.FromDto(x, CurrentContext))
            .Where(x => !x.IsActiveManagementKey)
            .ToImmutableList();
    }

    public async Task OnLockAsync()
    {
        var selectedApiKeyId = HttpContextAccessor.HttpContext!.Request.Form[SelectedApiKeyIdField].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await ManagementClient.LockApiKeyAsync(AppId, selectedApiKeyId);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyLocked,
                $"Locked API key '{selectedApiKeyId}' for application {AppId}.",
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

    public async Task OnUnlockAsync()
    {
        var selectedApiKeyId = HttpContextAccessor.HttpContext!.Request.Form["SelectedApiKeyId"].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await ManagementClient.UnlockApiKeyAsync(AppId, selectedApiKeyId);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyUnlocked,
                $"Unlocked API key '{selectedApiKeyId}' for application {AppId}.",
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

    public async Task OnDeleteAsync()
    {
        var selectedApiKeyId = HttpContextAccessor.HttpContext!.Request.Form["SelectedApiKeyId"].ToString();
        if (string.IsNullOrEmpty(selectedApiKeyId))
        {
            throw new ArgumentNullException(nameof(selectedApiKeyId));
        }

        try
        {
            await ManagementClient.DeleteApiKeyAsync(AppId, selectedApiKeyId);

            var eventDto = new OrganizationEventDto(HttpContextAccessor.HttpContext!.Request.HttpContext.User.GetId(),
                EventType.AdminApiKeyDeleted,
                $"Deleted API key '{selectedApiKeyId}' for application {AppId}.",
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
    
    public async Task<IActionResult> OnPostLockApiKeyAsync()
    {
        try
        {
            await ApiKeysModel.OnLockAsync();
            return RedirectToPage();
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }

    public async Task<IActionResult> OnPostUnlockApiKeyAsync()
    {
        try
        {
            await ApiKeysModel.OnUnlockAsync();
            return RedirectToPage();
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }

    public async Task<IActionResult> OnPostDeleteApiKeyAsync()
    {
        try
        {
            await ApiKeysModel.OnDeleteAsync();
            return RedirectToPage();
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Something unexpected occured. Please try again later." });
        }
    }

    public sealed class CreateFormModel
    {
        public string? Type { get; set; }
    }
}