using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Common.Models.MDS;
using Passwordless.Common.Models.Reporting;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{
    Task<ApplicationEventLogResponse> GetApplicationEventLog(int pageNumber, int pageSize);
    Task<IEnumerable<PeriodicCredentialReportResponse>> GetPeriodicCredentialReportsAsync(PeriodicCredentialReportRequest request);

    /// <summary>
    /// Retrieve a list of all attestation types in the FIDO2 MDS.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<string>> GetAttestationTypesAsync();

    /// <summary>
    /// Retrieve a list of all certification statuses in the FIDO2 MDS.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<string>> GetCertificationStatusesAsync();

    /// <summary>
    /// Get a list of all authenticators in the FIDO2 MDS.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IEnumerable<EntryResponse>> GetMetaDataStatementEntriesAsync(EntriesRequest request);

    /// <summary>
    /// Returns a list of configured authenticators for the current app. If the list is empty, all authenticators are allowed.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IEnumerable<ConfiguredAuthenticatorResponse>> GetConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request);

    /// <summary>
    /// Add specified authenticators to the whitelist.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task WhitelistAuthenticatorsAsync(WhitelistAuthenticatorsRequest request);

    /// <summary>
    /// Remove specified authenticators from the whitelist.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task DelistAuthenticatorsAsync(DelistAuthenticatorsRequest request);
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

    public async Task<IEnumerable<string>> GetAttestationTypesAsync()
    {
        var response = await _client.GetAsync("/mds/attestation-types");
        return (await response.Content.ReadFromJsonAsync<IEnumerable<string>>())!;
    }

    public async Task<IEnumerable<string>> GetCertificationStatusesAsync()
    {
        var response = await _client.GetAsync("/mds/certification-statuses");
        return (await response.Content.ReadFromJsonAsync<IEnumerable<string>>())!;
    }

    public async Task<IEnumerable<EntryResponse>> GetMetaDataStatementEntriesAsync(EntriesRequest request)
    {
        var queryBuilder = new QueryBuilder();
        if (request.AttestationTypes != null)
        {
            foreach (var attestationType in request.AttestationTypes)
            {
                queryBuilder.Add(nameof(request.AttestationTypes), attestationType);
            }
        }
        if (request.CertificationStatuses != null)
        {
            foreach (var certificationStatus in request.CertificationStatuses)
            {
                queryBuilder.Add(nameof(request.CertificationStatuses), certificationStatus);
            }
        }
        var q = queryBuilder.ToQueryString();
        return (await _client.GetFromJsonAsync<EntryResponse[]>($"/mds/entries{q}"))!;
    }

    public async Task<IEnumerable<ConfiguredAuthenticatorResponse>> GetConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request)
    {
        var queryBuilder = new QueryBuilder();
        queryBuilder.Add(nameof(request.IsAllowed), request.IsAllowed.ToString());
        var q = queryBuilder.ToQueryString();
        return (await _client.GetFromJsonAsync<ConfiguredAuthenticatorResponse[]>($"/authenticators/list{q}"))!;
    }

    public async Task WhitelistAuthenticatorsAsync(WhitelistAuthenticatorsRequest request)
    {
        var response = await _client.PostAsJsonAsync("/authenticators/whitelist", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DelistAuthenticatorsAsync(DelistAuthenticatorsRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"{_client.BaseAddress}authenticators/delist")
        };
        var response = await _client.SendAsync(httpRequestMessage);
        response.EnsureSuccessStatusCode();
    }
}