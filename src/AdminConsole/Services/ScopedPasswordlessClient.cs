using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.Net;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{
    Task<ApplicationAuditLogResponse> GetApplicationAuditLog();
}

public class ScopedPasswordlessClient : PasswordlessClient, IScopedPasswordlessClient
{
    private readonly HttpClient _client;
    private readonly ICurrentContext _currentContext;

    public ScopedPasswordlessClient(HttpClient httpClient, ICurrentContext context) : base(httpClient)
    {
        httpClient.DefaultRequestHeaders.Remove("ApiSecret");
        httpClient.DefaultRequestHeaders.Add("ApiSecret", context.ApiSecret);

        _client = httpClient;
        _currentContext = context;
    }

    public async Task<ApplicationAuditLogResponse> GetApplicationAuditLog()
    {
        var response = await _client.GetAsync($"events/{_currentContext.AppId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ApplicationAuditLogResponse>();
    }
}