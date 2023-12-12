using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{
    Task<ApplicationEventLogResponse> GetApplicationEventLog(int pageNumber, int pageSize);
}

public class ScopedPasswordlessClient : PasswordlessClient, IScopedPasswordlessClient
{
    private readonly HttpClient _client;

    public ScopedPasswordlessClient(
        HttpClient httpClient,
        IOptions<PasswordlessManagementOptions> options,
        ICurrentContext context)
        : base(new PasswordlessOptions
        {
            ApiSecret = context.ApiSecret!,
            ApiUrl = options.Value.InternalApiUrl,
        })
    {
        _client = httpClient;

        // can be dropped when call below is moved to the SDK.
        _client.DefaultRequestHeaders.Remove("ApiSecret");
        _client.DefaultRequestHeaders.Add("ApiSecret", context.ApiSecret);
    }

    public async Task<ApplicationEventLogResponse> GetApplicationEventLog(int pageNumber, int pageSize)
    {
        var response = await _client.GetAsync($"events?pageNumber={pageNumber}&numberOfResults={pageSize}");
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ApplicationEventLogResponse>())!;
    }
}