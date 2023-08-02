using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Passwordless.Net.Models;

namespace Passwordless.Net;

/// <summary>
/// TODO: FILL IN
/// </summary>
[DebuggerDisplay("{DebuggerToString()}")]
public class PasswordlessClient : IPasswordlessClient
{
    private readonly HttpClient _client;

    public static PasswordlessClient Create(PasswordlessOptions options, IHttpClientFactory factory)
    {
        var client = factory.CreateClient();
        client.BaseAddress = new Uri(options.ApiUrl);
        client.DefaultRequestHeaders.Add("ApiSecret", options.ApiSecret);
        return new PasswordlessClient(client);
    }

    public PasswordlessClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<RegisterTokenResponse> CreateRegisterTokenAsync(RegisterOptions registerOptions, CancellationToken cancellationToken = default)
    {
        var res = await _client.PostAsJsonAsync("register/token", registerOptions, cancellationToken);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<RegisterTokenResponse>(options: null, cancellationToken))!;
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
        var response = await _client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<VerifiedUser>();
            return res;
        }

        return null;
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _client.PostAsJsonAsync("users/delete", new { UserId = userId }, cancellationToken);
    }

    public async Task<List<PasswordlessUserSummary>?> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetFromJsonAsync<ListResponse<PasswordlessUserSummary>>("users/list", cancellationToken);
        return response!.Values;
    }

    public async Task<List<AliasPointer>> ListAliasesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetFromJsonAsync<ListResponse<AliasPointer>>($"alias/list?userid={userId}", cancellationToken);
        return response!.Values;
    }


    public async Task<List<Credential>> ListCredentialsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetFromJsonAsync<ListResponse<Credential>>($"credentials/list?userid={userId}", cancellationToken);
        return response!.Values;
    }

    public async Task DeleteCredentialAsync(string id, CancellationToken cancellationToken = default)
    {
        await _client.PostAsJsonAsync("credentials/delete", new { CredentialId = id }, cancellationToken);
    }

    public async Task DeleteCredentialAsync(byte[] id, CancellationToken cancellationToken = default)
    {
        await DeleteCredentialAsync(Base64Url.Encode(id), cancellationToken);
    }

    public async Task<UsersCount> GetUsersCountAsync(CancellationToken cancellationToken = default)
    {
        return (await _client.GetFromJsonAsync<UsersCount>("users/count", cancellationToken))!;
    }

    private string DebuggerToString()
    {
        var sb = new StringBuilder();
        sb.Append("ApiUrl = ");
        sb.Append(_client.BaseAddress);
        if (_client.DefaultRequestHeaders.TryGetValues("ApiSecret", out var values))
        {
            var apiSecret = values.First();
            if (apiSecret.Length > 5)
            {
                sb.Append(' ');
                sb.Append("ApiSecret = ");
                sb.Append("***");
                sb.Append(apiSecret.Substring(apiSecret.Length - 4));
            }
        }
        else
        {
            sb.Append(' ');
            sb.Append("ApiSecret = (null)");
        }

        return sb.ToString();
    }

    public class ListResponse<T>
    {
        public List<T> Values { get; set; } = null!;
    }

    public sealed class Base64UrlConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!reader.HasValueSequence)
            {
                return Base64Url.DecodeUtf8(reader.ValueSpan);
            }
            return Base64Url.Decode(reader.GetString().AsSpan());
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Base64Url.Encode(value));
        }
    }
}
