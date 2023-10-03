using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

namespace Passwordless.AdminConsole.Services;

public class PasswordlessManagementClient : IPasswordlessManagementClient
{
    private readonly HttpClient _client;

    public PasswordlessManagementClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<NewAppResponse> CreateApplication(string appId, NewAppOptions registerOptions)
    {
        var res = await _client.PostAsJsonAsync($"/admin/apps/{appId}/create", registerOptions);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<NewAppResponse>();
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
        var response = await _client.GetAsync($"apps/delete/cancel/{applicationId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CancelApplicationDeletionResponse>();
    }

    public async Task SetFeaturesAsync(string appId, SetApplicationFeaturesRequest request)
    {
        var response = await _client.PostAsJsonAsync($"admin/apps/{appId}/features", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AppFeatureDto> GetFeaturesAsync(string appId)
    {
        var response = await _client.GetAsync($"admin/apps/{appId}/features");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AppFeatureDto>();
        return result;
    }
}