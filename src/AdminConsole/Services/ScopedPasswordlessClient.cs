using Passwordless.Net;
using static Passwordless.Net.PasswordlessClient;
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

    public async Task<RegisterTokenResponse> CreateRegisterToken(RegisterOptions registerOptions)
    {
        var res = await PostAsync("register/token", registerOptions);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<RegisterTokenResponse>())!;
    }

    public async Task DeleteCredential(string id)
    {
        await PostAsync("credentials/delete", new { CredentialId = id });
    }

    public async Task DeleteCredential(byte[] id)
    {
        await DeleteCredential(Base64Url.Encode(id));
    }

    public async Task DeleteUserAsync(string userId)
    {
        await PostAsync("users/delete", new { UserId = userId });
    }

    public async Task<List<AliasPointer>> ListAliases(string userId)
    {
        var response = await GetAsync<PC.ListResponse<AliasPointer>>($"alias/list?userid={userId}");
        return response!.Values;
    }

    public async Task<List<Credential>> ListCredentials(string userId)
    {
        var response = await GetAsync<PC.ListResponse<Credential>>($"credentials/list?userid={userId}");
        return response!.Values;
    }

    public async Task<List<PasswordlessUserSummary>?> ListUsers()
    {
        var response = await GetAsync<PC.ListResponse<PasswordlessUserSummary>>("users/list");
        return response!.Values;
    }

    public async Task<VerifiedUser?> VerifyToken(string verifyToken)
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
        var response = await SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<VerifiedUser>();
            return res;
        }

        return null;
    }

    private Task<HttpResponseMessage> PostAsync<T>(string? requestUri, T value)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value),
        };
        return SendAsync(request);
    }

    private async Task<T?> GetAsync<T>(string? requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAsync(request);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        request.Headers.Add("ApiSecret", _currentContext.ApiSecret);
        return _client.SendAsync(request);
    }
}