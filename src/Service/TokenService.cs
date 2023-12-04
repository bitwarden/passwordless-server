using System.Security.Cryptography;
using System.Text;
using Fido2NetLib;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public class TokenService : ITokenService
{
    private readonly string _tenant;
    private readonly ILogger _log;
    private readonly IConfiguration _config;
    private readonly ITenantStorage _storage;

    private Dictionary<int, string>? _alternatives;

    public TokenService(ILogger log, IConfiguration config, ITenantStorage storage)
    {
        _tenant = storage.Tenant;
        _log = log;
        _config = config;
        _storage = storage;
    }

    private Key DeriveKey(string tenant, IConfiguration config, byte[] keyBytes)
    {
        var envSaltString = config["SALT_TOKEN"];
        Span<byte> salt = Convert.FromBase64String(envSaltString).AsSpan();

        if (string.IsNullOrEmpty(envSaltString))
        {
            // if no envsalt, use the codesalt32 bytes.
            _log.LogError("SALT_TOKEN env variable is not set. For production it is recommended to use a base64 encoded string of 32 random bytes");
            throw new Exception("SALT_TOKEN environment variable is missing. Should be a base64 string");
        }

        // bind this key to a specific version and to the tenant
        var version = "passwordless-1.0";
        var info = Encoding.UTF8.GetBytes(version + tenant);

        // create the key
        return KeyDerivationAlgorithm.HkdfSha256.DeriveKey(keyBytes, salt, info, MacAlgorithm.HmacSha256);
    }

    public async Task<T> DecodeTokenAsync<T>(string token, string prefix, bool contractless = false)
    {
        if (token == null)
        {
            throw new ApiException(
                "missing_token",
                $"This operation requires a token that starts with '{prefix}' to be passed.",
                400
            );
        }

        if (prefix != null)
        {
            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring(prefix.Length);
            }
            else
            {
                var invalidInput = token[..Math.Min(10, token.Length)];
                _log.LogWarning("Could not remove prefix={prefix}, token started with {InvalidInput}", prefix,
                    invalidInput);
                throw new ApiException("invalid_token",
                    $"The token you sent was not correct. The token used for this endpoint should start with '{prefix}'. Make sure you are not sending the wrong value. The value you sent started with '{invalidInput}'",
                    400);
            }
        }

        MacEnvelope envelope;
        try
        {
            var envelopeBytes = Base64Url.Decode(token);
            envelope = MessagePackSerializer.Deserialize<MacEnvelope>(envelopeBytes);
        }
        // Can happen if the token starts with the right prefix, but is otherwise syntactically incorrect
        catch
        {
            _log.LogError("Could not decode token={token}", token);

            throw new ApiException(
                "invalid_token_format",
                "The token you supplied was not formatted correctly. It should be valid base64url.",
                400
            );
        }

        _log.LogInformation("Decoding using keyId={keyId}", envelope.KeyId);
        Key key;
        try
        {
            key = await GetKeyByKeyIdAsync(envelope.KeyId);
        }
        // Can happen if the key is syntactically correct, but not issued by us
        catch
        {
            _log.LogError("Could not recognize token={token}", token);

            throw new ApiException(
                "invalid_token",
                "The token you supplied was not valid. It could be that the token has expired or that it was not issued for this tenant.",
                400
            );
        }

        var isOk = VerifyMac(key, envelope.Token, envelope.Mac);
        if (!isOk)
        {
            return default;
        }

        return contractless
            ? MessagePackSerializer.Deserialize<T>(envelope.Token, ContractlessStandardResolver.Options)
            : MessagePackSerializer.Deserialize<T>(envelope.Token);
    }

    private async Task<Key> GetKeyByKeyIdAsync(int keyId)
    {
        var keyinput = (await GetAlternativesAsync())[keyId];

        var keybytes = Convert.FromBase64String(keyinput);
        var key = DeriveKey(_tenant, _config, keybytes);

        return key;
    }

    private async Task<Tuple<Key, int>> GetRandomKeyAsync()
    {
        // Key used to sign tokens
        // get newest key from alternatives (it's sorted desc)
        var kvp = (await GetAlternativesAsync()).First();
        var keyinput = (await GetAlternativesAsync())[kvp.Key];

        var keybytes = Convert.FromBase64String(keyinput);

        var key = DeriveKey(_tenant, _config, keybytes);
        return new Tuple<Key, int>(key, kvp.Key);
    }

    public async Task<string> EncodeTokenAsync<T>(T token, string prefix, bool contractless = false)
    {
        byte[] msgpack;
        if (contractless)
        {
            msgpack = MessagePackSerializer.Serialize(token, ContractlessStandardResolver.Options);
        }
        else
        {
            msgpack = MessagePackSerializer.Serialize<T>(token);
        }

        (Key key, int keyId) = await GetRandomKeyAsync();

        _log.LogInformation("Encoding using keyId={keyId}", keyId);
        var mac = CreateMac(key, msgpack);

        var envelope = new MacEnvelope { Mac = mac, Token = msgpack, KeyId = keyId };
        var envelop_binary = MessagePackSerializer.Serialize(envelope);
        var envelop_binary_b64 = Base64Url.Encode(envelop_binary);

        if (!string.IsNullOrEmpty(prefix))
        {
            return prefix + envelop_binary_b64;
        }

        return envelop_binary_b64;
    }

    /// <summary>
    /// MacEnvelope - must be public to allow serialization
    /// </summary>
    [MessagePackObject]
    public class MacEnvelope
    {
        [Key(0)]
        public byte[] Mac { get; set; }
        [Key(1)]
        public byte[] Token { get; set; }
        [Key(2)]
        public int KeyId { get; set; }
    }

    private byte[] CreateMac(Key key, byte[] data)
    {
        var mac = MacAlgorithm.HmacSha256.Mac(key, data);
        return mac;
    }

    private bool VerifyMac(Key key, byte[] data, byte[] mac)
    {
        var result = MacAlgorithm.HmacSha256.Verify(key, data, mac);

        if (!result)
        {
            throw new Exception("Bad mac");
        }

        return result;
    }

    private async Task<Dictionary<int, string>> GetAlternativesAsync()
    {
        if (_alternatives == null)
        {
            var keys = await _storage.GetTokenKeys();

            // transform our list to a dictionary
            var alternatives = keys.OrderByDescending(x => x.CreatedAt).ToDictionary(k => k.KeyId, k => k.KeyMaterial);

            // Rotate keys every 7 day
            // Remove old keys after 30 days (side effect: Tokens maximum life length is 30 days).
            if (!alternatives.Any() || (DateTime.UtcNow - keys.First().CreatedAt).TotalDays > 7)
            {
                var random32Bytes = RandomNumberGenerator.GetBytes(32);
                var keyInputMaterial = Convert.ToBase64String(random32Bytes);

                // todo: Handle 409 exception from storage? Storage will throw if keyid is duplicate.
                var keyId = RandomNumberGenerator.GetInt32(int.MaxValue);
                await _storage.AddTokenKey(new TokenKey { CreatedAt = DateTime.UtcNow, KeyId = keyId, KeyMaterial = keyInputMaterial });
                alternatives.Add(keyId, keyInputMaterial);

                try
                {
                    await _storage.RemoveExpiredTokenKeys(CancellationToken.None);
                }
                catch (Exception)
                {
                    _log.LogError("Failed to remove old key, account={accountName}", _tenant);
                }
            }

            _alternatives = alternatives;
        }

        return _alternatives;
    }
}