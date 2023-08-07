using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.Net;

namespace Passwordless.AdminConsole.Services;

public class PasswordlessManagementClient
{
    private readonly HttpClient _client;

    public PasswordlessManagementClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<NewAppResponse> CreateApplication(NewAppOptions registerOptions)
    {
        var res = await _client.PostAsJsonAsync("apps/create", registerOptions);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<NewAppResponse>();
    }

    public async Task<MarkDeleteApplicationResponse> MarkDeleteApplication(MarkDeleteApplicationRequest request)
    {
        var response = await _client.PostAsJsonAsync("apps/mark-delete", new { request.AppId, request.DeletedBy });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MarkDeleteApplicationResponse>();
    }

    public async Task<ICollection<string>> GetApplicationsPendingDeletion()
    {
        var response = await _client.GetAsync("apps/list-pending-deletion");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ICollection<string>>();
    }
    
    public async Task<bool> DeleteApplicationAsync(string application)
    {
        var request = new { appId = application };
        var res = await _client.PostAsJsonAsync("apps/delete", request);
        var why = await res.Content.ReadAsStringAsync();
        return res.IsSuccessStatusCode;
    }

    public async Task<ApplicationSummaryResponse> GetApplicationSummary(string applicationId)
    {
        var response = await _client.GetAsync($"apps/summary/{applicationId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApplicationSummaryResponse>();
    }
}