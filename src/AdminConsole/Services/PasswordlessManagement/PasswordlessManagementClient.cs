using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public class PasswordlessManagementClient : IPasswordlessManagementClient
{
    private readonly HttpClient _client;

    public PasswordlessManagementClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<CreateAppResultDto> CreateApplication(string appId, CreateAppDto options)
    {
        var res = await _client.PostAsJsonAsync($"/admin/apps/{appId}/create", options);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<CreateAppResultDto>();
    }

    public async Task<MarkDeleteApplicationResponse> MarkDeleteApplication(MarkDeleteApplicationRequest request)
    {
        var response = await _client.PostAsJsonAsync($"admin/apps/{request.AppId}/mark-delete", new { request.DeletedBy });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MarkDeleteApplicationResponse>();
    }

    public async Task<ICollection<string>> ListApplicationsPendingDeletionAsync()
    {
        var response = await _client.GetAsync("apps/list-pending-deletion");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ICollection<string>>();
    }

    public async Task<bool> DeleteApplicationAsync(string application)
    {
        var res = await _client.DeleteAsync($"admin/apps/{application}");
        return res.IsSuccessStatusCode;
    }

    public async Task<CancelApplicationDeletionResponse> CancelApplicationDeletion(string applicationId)
    {
        var response = await _client.PostAsync($"admin/apps/{applicationId}/cancel-delete", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CancelApplicationDeletionResponse>();
    }

    public async Task SetFeaturesAsync(string appId, ManageFeaturesRequest request)
    {
        var response = await _client.PostAsJsonAsync($"admin/apps/{appId}/features", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AppFeatureResponse> GetFeaturesAsync(string appId)
    {
        var response = await _client.GetAsync($"admin/apps/{appId}/features");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AppFeatureResponse>();
        return result;
    }

    public async Task<ICollection<ApiKeyResponse>> GetApiKeysAsync(string appId)
    {
        var response = await _client.GetAsync($"admin/apps/{appId}/api-keys");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ICollection<ApiKeyResponse>>();
        return result;
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreatePublicKeyRequest request)
    {
        var response = await _client.PostAsJsonAsync($"admin/apps/{appId}/public-keys", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        return result!;
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreateSecretKeyRequest request)
    {
        var response = await _client.PostAsJsonAsync($"admin/apps/{appId}/secret-keys", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        return result!;
    }

    public async Task LockApiKeyAsync(string appId, string apiKeyId)
    {
        var response = await _client.PostAsync($"admin/apps/{appId}/api-keys/{apiKeyId}/lock", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnlockApiKeyAsync(string appId, string apiKeyId)
    {
        var response = await _client.PostAsync($"admin/apps/{appId}/api-keys/{apiKeyId}/unlock", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteApiKeyAsync(string appId, string apiKeyId)
    {
        var response = await _client.DeleteAsync($"admin/apps/{appId}/api-keys/{apiKeyId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task EnableGenerateSignInTokenEndpointAsync(string appId, string performedBy)
    {
        var response = await _client.PostAsJsonAsync($"admin/apps/{appId}/sign-in-generate-token-endpoint/enable", new
        {
            PerformedBy = performedBy
        });
        response.EnsureSuccessStatusCode();
    }
    
    public async Task DisableGenerateSignInTokenEndpointAsync(string appId, string performedBy)
    {
        var response = await _client.PostAsJsonAsync($"admin/apps/{appId}/sign-in-generate-token-endpoint/disable", new
        {
            PerformedBy = performedBy
        });
        response.EnsureSuccessStatusCode();
    }
}