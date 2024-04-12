using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Common.Models.Reporting;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{
    Task<ApplicationEventLogResponse> GetApplicationEventLog(int pageNumber, int pageSize);
    Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest request);
    Task<IEnumerable<PeriodicActiveUserReportResponse>> GetPeriodicActiveUserReportsAsync(PeriodicActiveUserReportRequest request);

    /// <summary>
    /// Returns a list of configured authenticators for the current app. If the list is empty, all authenticators are allowed.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IEnumerable<ConfiguredAuthenticatorResponse>> GetConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request);

    /// <summary>
    /// Add specified authenticators to the whitelist/blacklist.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task AddAuthenticatorsAsync(AddAuthenticatorsRequest request);

    /// <summary>
    /// Remove specified authenticators from the whitelist/blacklist.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task RemoveAuthenticatorsAsync(RemoveAuthenticatorsRequest request);

    Task SetFeaturesAsync(SetFeaturesRequest request);

    Task<GetAuthenticationConfigurationsResult> GetAuthenticationConfigurationsAsync();
    Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose);
    Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration);
    Task SaveAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration);
    Task DeleteAuthenticationConfigurationAsync(string purpose);
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
        using var response = await _client.PostAsJsonAsync($"/apps/features", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<GetAuthenticationConfigurationsResult> GetAuthenticationConfigurationsAsync() =>
        (await _client.GetFromJsonAsync<GetAuthenticationConfigurationsResult>("authentication-configurations"))!;

    public async Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose)
    {
        return await _client.GetFromJsonAsync<AuthenticationConfigurationDto?>($"authentication-configuration?purpose={Uri.EscapeDataString(purpose)}");
    }

    public async Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration)
    {
        using var response = await _client.PostAsJsonAsync("authentication-configuration/add", configuration);
        response.EnsureSuccessStatusCode();
    }

    public async Task SaveAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest configuration)
    {
        using var response = await _client.PostAsJsonAsync("authentication-configuration", configuration);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAuthenticationConfigurationAsync(string purpose)
    {
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "authentication-configuration");
        deleteRequest.Content = new StringContent(
            // lang=json
            $$"""
              {
                "purpose": "{{purpose}}"
              }
              """,
            Encoding.UTF8,
            MediaTypeNames.Application.Json
        );
        using var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.EnsureSuccessStatusCode();
    }
}