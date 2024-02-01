using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Reporting;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{
    Task<ApplicationEventLogResponse> GetApplicationEventLog(int pageNumber, int pageSize);
    Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest request);

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
}

public class ScopedPasswordlessClient : PasswordlessClient, IScopedPasswordlessClient
{
    private readonly HttpClient _client;
    private readonly ICurrentContext _currentContext;

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
        _currentContext = context;

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
        var response = await _client.GetAsync($"/apps/{_currentContext.AppId}/reporting/credentials/periodic{q}");
        response.EnsureSuccessStatusCode();

        var rest = (await response.Content.ReadFromJsonAsync<IEnumerable<PeriodicCredentialReportResponse>>())!;
        return rest;
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
}