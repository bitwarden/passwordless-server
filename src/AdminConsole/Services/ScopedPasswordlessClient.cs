using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.Net;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{
    Task<ApplicationAuditLogResponse> GetApplicationAuditLog(int pageNumber, int pageSize);
}

public class ScopedPasswordlessClient : PasswordlessClient, IScopedPasswordlessClient
{
    private readonly HttpClient _client;

    public ScopedPasswordlessClient(HttpClient httpClient, ICurrentContext context) : base(httpClient)
    {
        httpClient.DefaultRequestHeaders.Remove("ApiSecret");
        httpClient.DefaultRequestHeaders.Add("ApiSecret", context.ApiSecret);

        _client = httpClient;
    }

    public async Task<ApplicationAuditLogResponse> GetApplicationAuditLog(int pageNumber, int pageSize)
    {
        var response = await _client.GetAsync($"events?pageNumber={pageNumber}&numberOfResults={pageSize}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ApplicationAuditLogResponse>();
    }
}