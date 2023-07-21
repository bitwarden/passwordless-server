using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Passwordless.Net;

public interface IPasswordlessClient
{
    Task<RegisterTokenResponse> CreateRegisterToken(RegisterOptions registerOptions);
    Task DeleteCredential(string id);
    Task DeleteCredential(byte[] id);
    Task<List<AliasPointer>> ListAliases(string userId);
    Task<List<Credential>> ListCredentials(string userId);
    Task<List<PasswordlessUserSummary>?> ListUsers();
    Task<VerifiedUser?> VerifyToken(string verifyToken);
    Task DeleteUserAsync(string userId);
}

public class RegisterTokenResponse
{
    public string Token { get; set; }
}

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

    public async Task<RegisterTokenResponse> CreateRegisterToken(RegisterOptions registerOptions)
    {
        var res = await _client.PostAsJsonAsync("register/token", registerOptions);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<RegisterTokenResponse>())!;
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
        var response = await _client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<VerifiedUser>();
            return res;
        }

        return null;
    }

    public async Task DeleteUserAsync(string userId)
    {
        await _client.PostAsJsonAsync("users/delete", new { UserId = userId });
    }

    public async Task<List<PasswordlessUserSummary>?> ListUsers()
    {
        var response = await _client.GetFromJsonAsync<ListResponse<PasswordlessUserSummary>>("users/list");
        return response!.Values;
    }

    public async Task<List<AliasPointer>> ListAliases(string userId)
    {
        var response = await _client.GetFromJsonAsync<ListResponse<AliasPointer>>($"alias/list?userid={userId}");
        return response!.Values;
    }


    public async Task<List<Credential>> ListCredentials(string userId)
    {
        var response = await _client.GetFromJsonAsync<ListResponse<Credential>>($"credentials/list?userid={userId}");
        return response!.Values;
    }

    public async Task DeleteCredential(string id)
    {
        await _client.PostAsJsonAsync("credentials/delete", new { CredentialId = id });
    }

    public async Task DeleteCredential(byte[] id)
    {
        await DeleteCredential(Base64Url.Encode(id));
    }

    public async Task<UsersCount> GetUsersCount()
    {
        return (await _client.GetFromJsonAsync<UsersCount>("users/count"))!;
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
                sb.Append(apiSecret.AsSpan(apiSecret.Length - 4));
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
            return Base64Url.Decode(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Base64Url.Encode(value));
        }
    }

    public static class Base64Url
    {
        /// <summary>
        /// Converts arg data to a Base64Url encoded string.
        /// </summary>
        public static string Encode(ReadOnlySpan<byte> arg)
        {
            int minimumLength = (int)(((long)arg.Length + 2L) / 3 * 4);
            char[] array = ArrayPool<char>.Shared.Rent(minimumLength);
            Convert.TryToBase64Chars(arg, array, out var charsWritten);
            Span<char> span = array.AsSpan(0, charsWritten);
            for (int i = 0; i < span.Length; i++)
            {
                ref char reference = ref span[i];
                switch (reference)
                {
                    case '+':
                        reference = '-';
                        break;
                    case '/':
                        reference = '_';
                        break;
                }
            }
            int num = span.IndexOf('=');
            if (num > -1)
            {
                span = span.Slice(0, num);
            }
            string result = new string(span);
            ArrayPool<char>.Shared.Return(array, clearArray: true);
            return result;
        }

        /// <summary>
        /// Decodes a Base64Url encoded string to its raw bytes.
        /// </summary>
        public static byte[] Decode(ReadOnlySpan<char> text)
        {
            int num = (text.Length % 4) switch
            {
                2 => 2,
                3 => 1,
                _ => 0,
            };
            int num2 = text.Length + num;
            char[] array = ArrayPool<char>.Shared.Rent(num2);
            text.CopyTo(array);
            for (int i = 0; i < text.Length; i++)
            {
                ref char reference = ref array[i];
                switch (reference)
                {
                    case '-':
                        reference = '+';
                        break;
                    case '_':
                        reference = '/';
                        break;
                }
            }
            switch (num)
            {
                case 1:
                    array[num2 - 1] = '=';
                    break;
                case 2:
                    array[num2 - 1] = '=';
                    array[num2 - 2] = '=';
                    break;
            }
            byte[] result = Convert.FromBase64CharArray(array, 0, num2);
            ArrayPool<char>.Shared.Return(array, clearArray: true);
            return result;
        }

        /// <summary>
        /// Decodes a Base64Url encoded string to its raw bytes.
        /// </summary>
        public static byte[] DecodeUtf8(ReadOnlySpan<byte> text)
        {
            int num = (text.Length % 4) switch
            {
                2 => 2,
                3 => 1,
                _ => 0,
            };
            int num2 = text.Length + num;
            byte[] array = ArrayPool<byte>.Shared.Rent(num2);
            text.CopyTo(array);
            for (int i = 0; i < text.Length; i++)
            {
                ref byte reference = ref array[i];
                switch (reference)
                {
                    case 45:
                        reference = 43;
                        break;
                    case 95:
                        reference = 47;
                        break;
                }
            }
            switch (num)
            {
                case 1:
                    array[num2 - 1] = 61;
                    break;
                case 2:
                    array[num2 - 1] = 61;
                    array[num2 - 2] = 61;
                    break;
            }
            Base64.DecodeFromUtf8InPlace(array.AsSpan(0, num2), out var bytesWritten);
            byte[] result = array.AsSpan(0, bytesWritten).ToArray();
            ArrayPool<byte>.Shared.Return(array, clearArray: true);
            return result;
        }


    }
}