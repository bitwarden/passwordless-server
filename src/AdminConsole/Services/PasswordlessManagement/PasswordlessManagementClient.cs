using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public class PasswordlessManagementClient(HttpClient http) : IPasswordlessManagementClient
{
    public async Task<CreateAppResultDto> CreateApplicationAsync(string appId, CreateAppDto options)
    {
        using var response = await http.PostAsJsonAsync(
            $"/admin/apps/{Uri.EscapeDataString(appId)}/create",
            options
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateAppResultDto>();
    }

    public async Task<MarkDeleteApplicationResponse> MarkDeleteApplicationAsync(MarkDeleteApplicationRequest request)
    {
        using var response = await http.PostAsJsonAsync(
            $"admin/apps/{Uri.EscapeDataString(request.AppId)}/mark-delete",
            new { request.DeletedBy }
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MarkDeleteApplicationResponse>();
    }

    public async Task<ICollection<string>> ListApplicationsPendingDeletionAsync()
    {
        using var response = await http.GetAsync("apps/list-pending-deletion");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ICollection<string>>();
    }

    public async Task<bool> DeleteApplicationAsync(string appId)
    {
        using var response = await http.DeleteAsync($"admin/apps/{Uri.EscapeDataString(appId)}");
        return response.IsSuccessStatusCode;
    }

    public async Task<CancelApplicationDeletionResponse> CancelApplicationDeletionAsync(string appId)
    {
        using var response = await http.PostAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/cancel-delete",
            null
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CancelApplicationDeletionResponse>();
    }

    public async Task SetFeaturesAsync(string appId, ManageFeaturesRequest request)
    {
        using var response = await http.PostAsJsonAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/features",
            request
        );

        response.EnsureSuccessStatusCode();
    }

    public async Task<AppFeatureResponse> GetFeaturesAsync(string appId)
    {
        using var response = await http.GetAsync($"admin/apps/{Uri.EscapeDataString(appId)}/features");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AppFeatureResponse>();
    }

    public async Task<ICollection<ApiKeyResponse>> GetApiKeysAsync(string appId)
    {
        using var response = await http.GetAsync($"admin/apps/{Uri.EscapeDataString(appId)}/api-keys");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ICollection<ApiKeyResponse>>();
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreatePublicKeyRequest request)
    {
        using var response = await http.PostAsJsonAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/public-keys",
            request
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreateSecretKeyRequest request)
    {
        using var response = await http.PostAsJsonAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/secret-keys",
            request
        );
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
    }

    public async Task LockApiKeyAsync(string appId, string apiKeyId)
    {
        using var response = await http.PostAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/api-keys/{Uri.EscapeDataString(apiKeyId)}/lock",
            null
        );

        response.EnsureSuccessStatusCode();
    }

    public async Task UnlockApiKeyAsync(string appId, string apiKeyId)
    {
        using var response = await http.PostAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/api-keys/{Uri.EscapeDataString(apiKeyId)}/unlock",
            null
        );

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteApiKeyAsync(string appId, string apiKeyId)
    {
        using var response = await http.DeleteAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/api-keys/{Uri.EscapeDataString(apiKeyId)}"
        );

        response.EnsureSuccessStatusCode();
    }

    public async Task EnabledManuallyGeneratedTokensAsync(string appId, string performedBy)
    {
        using var response = await http.PostAsJsonAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/sign-in-generate-token-endpoint/enable",
            new { PerformedBy = performedBy }
        );

        response.EnsureSuccessStatusCode();
    }

    public async Task DisabledManuallyGeneratedTokensAsync(string appId, string performedBy)
    {
        using var response = await http.PostAsJsonAsync(
            $"admin/apps/{Uri.EscapeDataString(appId)}/sign-in-generate-token-endpoint/disable",
            new { PerformedBy = performedBy }
        );

        response.EnsureSuccessStatusCode();
    }

    public async Task<GetAppIdAvailabilityResponse> IsApplicationIdAvailableAsync(GetAppIdAvailabilityRequest request) =>
        (await http.GetFromJsonAsync<GetAppIdAvailabilityResponse>(
            $"admin/apps/{Uri.EscapeDataString(request.AppId)}/available"
        ))!;

    public async Task EnableMagicLinksAsync(string appId, string performedBy)
    {
        using var response = await http.PostAsJsonAsync($"admin/apps/{Uri.EscapeDataString(appId)}/magic-links/enable", new
        {
            PerformedBy = performedBy
        });
        response.EnsureSuccessStatusCode();
    }

    public async Task DisableMagicLinksAsync(string appId, string performedBy)
    {
        using var response = await http.PostAsJsonAsync($"admin/apps/{Uri.EscapeDataString(appId)}/magic-links/disable", new
        {
            PerformedBy = performedBy
        });
        response.EnsureSuccessStatusCode();
    }
}