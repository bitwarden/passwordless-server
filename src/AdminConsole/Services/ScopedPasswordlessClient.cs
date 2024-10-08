using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Common.Models.Reporting;

namespace Passwordless.AdminConsole.Services;

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
            ApiUrl = options.Value.ApiUrl,
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

    public async Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest request)
    {
        var queryBuilder = new QueryBuilder();
        if (request.From.HasValue)
        {
            queryBuilder.Add("from", request.From.Value.ToString("yyyy-MM-dd"));
        }
        if (request.To.HasValue)
        {
            queryBuilder.Add("to", request.To.Value.ToString("yyyy-MM-dd"));
        }

        var q = queryBuilder.ToQueryString();
        var response = await _client.GetAsync($"/reporting/credentials/periodic{q}");
        response.EnsureSuccessStatusCode();

        var rest = (await response.Content.ReadFromJsonAsync<IEnumerable<PeriodicCredentialReportResponse>>())!;
        return rest;
    }

    public async Task<IEnumerable<PeriodicActiveUserReportResponse>> GetPeriodicActiveUserReportsAsync(PeriodicActiveUserReportRequest request)
    {
        var queryBuilder = new QueryBuilder();
        if (request.From.HasValue)
        {
            queryBuilder.Add("from", request.From.Value.ToString("yyyy-MM-dd"));
        }
        if (request.To.HasValue)
        {
            queryBuilder.Add("to", request.To.Value.ToString("yyyy-MM-dd"));
        }

        var q = queryBuilder.ToQueryString();
        var response = await _client.GetAsync($"/reporting/active-users/periodic{q}");
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<IEnumerable<PeriodicActiveUserReportResponse>>())!;
    }

    public async Task<IEnumerable<ConfiguredAuthenticatorResponse>> GetConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request)
    {
        var queryBuilder = new QueryBuilder();
        queryBuilder.Add(nameof(request.IsAllowed), request.IsAllowed.ToString());
        var q = queryBuilder.ToQueryString();
        return (await _client.GetFromJsonAsync<ConfiguredAuthenticatorResponse[]>($"/authenticators/list{q}"))!;
    }

    public async Task AddAuthenticatorsAsync(AddAuthenticatorsRequest request)
    {
        using var response = await _client.PostAsJsonAsync("/authenticators/add", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAuthenticatorsAsync(RemoveAuthenticatorsRequest request)
    {
        using var response = await _client.PostAsJsonAsync("/authenticators/remove", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetFeaturesAsync(SetFeaturesRequest request)
    {
        using var response = await _client.PostAsJsonAsync("/apps/features", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<GetAuthenticationConfigurationsResult> GetAuthenticationConfigurationsAsync() =>
        (await _client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>("auth-configs/list"))!;

    public async Task<GetAuthenticationConfigurationsResult> GetAuthenticationConfigurationAsync(string purpose)
    {
        return (await _client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>($"auth-configs/list?purpose={Uri.EscapeDataString(purpose)}"))!;
    }

    public async Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration)
    {
        using var response = await _client.PostAsJsonAsync("auth-configs/add", configuration);
        response.EnsureSuccessStatusCode();
    }

    public async Task SaveAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration)
    {
        using var response = await _client.PostAsJsonAsync("auth-configs", configuration);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAuthenticationConfigurationAsync(DeleteAuthenticationConfigurationRequest configuration)
    {
        using var deleteResponse = await _client.PostAsJsonAsync("auth-configs/delete", configuration);
        deleteResponse.EnsureSuccessStatusCode();
    }
}