using Passwordless.Net;
using Passwordless.Net.Models;
using PC = Passwordless.Net.PasswordlessClient;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{

}

public class ScopedPasswordlessClient : IScopedPasswordlessClient
{
    private readonly HttpClient _client;
    private readonly ICurrentContext _currentContext;

    public ScopedPasswordlessClient(HttpClient httpClient, ICurrentContext currentContext)
    {
        _client = httpClient;
        _currentContext = currentContext;
    }

    public async Task<RegisterTokenResponse> CreateRegisterTokenAsync(RegisterOptions registerOptions, CancellationToken cancellationToken = default)
    {
        var res = await PostAsync("register/token", registerOptions, cancellationToken);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<RegisterTokenResponse>(options: null, cancellationToken))!;
    }

    public async Task DeleteCredentialAsync(string id, CancellationToken cancellationToken = default)
    {
        await PostAsync("credentials/delete", new { CredentialId = id }, cancellationToken);
    }

    public async Task DeleteCredentialAsync(byte[] id, CancellationToken cancellationToken = default)
    {
        await DeleteCredentialAsync(Base64Url.Encode(id), cancellationToken);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        await PostAsync("users/delete", new { UserId = userId }, cancellationToken);
    }

    public async Task<List<AliasPointer>> ListAliasesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<PC.ListResponse<AliasPointer>>($"alias/list?userid={userId}", cancellationToken);
        return response!.Values;
    }

    public async Task<List<Credential>> ListCredentialsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<PC.ListResponse<Credential>>($"credentials/list?userid={userId}", cancellationToken);
        return response!.Values;
    }

    public async Task<List<PasswordlessUserSummary>?> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<PC.ListResponse<PasswordlessUserSummary>>("users/list", cancellationToken);
        return response!.Values;
    }

    public async Task<VerifiedUser?> VerifyTokenAsync(string verifyToken, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "signin/verify")
        {
            Content = JsonContent.Create(new
            {
                token = verifyToken,
            }),
        };

        // We just want to return null if there is a problem.
        request.SkipErrorHandling();
        var response = await SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<VerifiedUser>();
            return res;
        }

        return null;
    }

    private Task<HttpResponseMessage> PostAsync<T>(string? requestUri, T value, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value),
        };
        return SendAsync(request, cancellationToken);
    }

    private async Task<T?> GetAsync<T>(string? requestUri, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAsync(request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(options: null, cancellationToken);
    }

    private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("ApiSecret", _currentContext.ApiSecret);
        return _client.SendAsync(request, cancellationToken);
    }
}