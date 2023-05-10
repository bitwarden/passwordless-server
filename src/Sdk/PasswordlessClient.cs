using System.Buffers;
using System.Buffers.Text;
using System.Net.Http.Json;
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
        return (await res.Content.ReadFromJsonAsync<RegisterTokenResponse>());
    }

    public async Task<VerifiedUser?> VerifyToken(string verifyToken)
    {
        var req = await _client.PostAsJsonAsync("signin/verify", new { token = verifyToken });

        // todo: replace with better error handling
        req.EnsureSuccessStatusCode();

        if (req.IsSuccessStatusCode)
        {
            var res = await req.Content.ReadFromJsonAsync<VerifiedUser>();
            return res;
        }

        return null;
    }

    public async Task DeleteUserAsync(string userId)
    {
        var req = await _client.PostAsJsonAsync("users/delete", new { UserId = userId });

        // todo: replace with better error handling
        req.EnsureSuccessStatusCode();
    }

    public async Task<List<PasswordlessUserSummary>?> ListUsers()
    {
        var req = await _client.GetAsync("users/list");

        // todo: replace with better error handling
        req.EnsureSuccessStatusCode();

        if (req.IsSuccessStatusCode)
        {
            var res = await req.Content.ReadFromJsonAsync<ListResponse<PasswordlessUserSummary>>();
            return res.Values;
        }

        return null;
    }

    public async Task<List<AliasPointer>> ListAliases(string userId)
    {
        var req = await _client.GetAsync($"alias/list?userid={userId}");
        req.EnsureSuccessStatusCode();

        var res = await req.Content.ReadFromJsonAsync<ListResponse<AliasPointer>>();


        return res.Values;
    }


    public async Task<List<Credential>> ListCredentials(string userId)
    {
        var req = await _client.GetAsync($"credentials/list?userid={userId}");

        req.EnsureSuccessStatusCode();

        var res = await req.Content.ReadFromJsonAsync<ListResponse<Credential>>();

        return res.Values;
    }

    public async Task DeleteCredential(string id)
    {
        var req = await _client.PostAsJsonAsync("credentials/delete", new { CredentialId = id });

        req.EnsureSuccessStatusCode();
    }

    public async Task DeleteCredential(byte[] id)
    {

        var req = await _client.PostAsJsonAsync("credentials/delete", new { CredentialId = Base64Url.Encode(id) });
        req.EnsureSuccessStatusCode();

    }

    public class ListResponse<T>
    {
        public List<T> Values { get; set; }
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

    public async Task<UsersCount> GetUsersCount()
    {
        var req = await _client.GetAsync("users/count");

        req.EnsureSuccessStatusCode();

        return await req.Content.ReadFromJsonAsync<UsersCount>();
    }
}