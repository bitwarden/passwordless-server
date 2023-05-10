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
using Passwordless.Service.Storage;

namespace Passwordless.Service;

public class TokenService
{
    private readonly string _tenant;
    private readonly ILogger _log;
    private readonly IConfiguration _config;
    private readonly ITenantStorage _storage;
    private Dictionary<int, string> alternatives;

    public TokenService(string tenant, ILogger log, IConfiguration config, ITenantStorage storage)
    {
        _tenant = tenant;
        _log = log;
        _config = config;
        _storage = storage;
    }

    public async Task Init()
    {
        var keys = await _storage.GetTokenKeys();

        // transform our list to a dictionary
        alternatives = keys.OrderByDescending(x => x.CreatedAt).ToDictionary(k => k.KeyId, k => k.KeyMaterial);

        // Rotate keys every 7 day
        // Remove old keys after 30 days (side effect: Tokens maximum life length is 30 days).
        if (alternatives.Count == 0 || (DateTime.UtcNow - keys.First().CreatedAt).TotalDays > 7)
        {
            var random32bytes = RandomNumberGenerator.GetBytes(32);
            var keyinputmaterial = Convert.ToBase64String(random32bytes);

            // todo: Handle 409 exception from storage? Storage will throw if keyid is duplicate.
            var keyId = RandomNumberGenerator.GetInt32(int.MaxValue);
            await _storage.AddTokenKey(new TokenKey() { CreatedAt = DateTime.UtcNow, KeyId = keyId, KeyMaterial = keyinputmaterial });
            alternatives.Add(keyId, keyinputmaterial);

            try
            {
                var oldKeys = keys.Where(x => (DateTime.UtcNow - x.CreatedAt).TotalDays > 30);
                await Task.WhenAll(oldKeys.Select(k => _storage.RemoveTokenKey(k.KeyId)));
            }
            catch (Exception)
            {
                _log.LogError("Failed to remove old key, account={accountName}", _tenant);
            }
        }
    }


    private Key DeriveKey(string tenant, IConfiguration config, byte[] keybytes)
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
        return KeyDerivationAlgorithm.HkdfSha256.DeriveKey(keybytes, salt, info, MacAlgorithm.HmacSha256);
    }

    public T DecodeToken<T>(string token, string prefix, bool contractless = false)
    {
        if (token == null) throw new ArgumentNullException("token");

        if (prefix != null)
        {
            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring(prefix.Length);
            }
            else
            {
                _log.LogWarning("Could not remove prefix={prefix}", prefix);
                // todo: Uncomment to require new token format. Should be safe to do after 2020-01-30:
                throw new ApiException($"The token you sent was not correct. The token used for this endpoint should start with '{prefix}'. Make sure you are not sending the wrong value.", 400);
            }
        }

        byte[] envelopeBytes;
        try
        {
            envelopeBytes = Base64Url.Decode(token);
        }
        catch (Exception)
        {
            _log.LogError("Could not decode token={token}", token);
            throw new ApiException("The token you supplied was not formatted correctly. It should be valid base64url.", 400);
        }
        var envelope = MessagePackSerializer.Deserialize<MacEnvelope>(envelopeBytes);

        _log.LogInformation("Decoding using keyId={keyId}", envelope.KeyId);
        var key = GetKeyByKeyId(envelope.KeyId);

        var isOK = VerifyMac(key, envelope.Token, envelope.Mac);

        if (!isOK) { return default; }


        T res;
        if (contractless)
        {
            res = MessagePackSerializer.Deserialize<T>(envelope.Token, ContractlessStandardResolver.Options);
        }
        else
        {
            res = MessagePackSerializer.Deserialize<T>(envelope.Token);

        }

        return res;
    }


    private Key GetKeyByKeyId(int keyId)
    {
        var keyinput = alternatives[keyId];

        var keybytes = Convert.FromBase64String(keyinput);
        var key = DeriveKey(_tenant, _config, keybytes);

        return key;
    }

    private Tuple<Key, int> GetRandomKey()
    {
        // Key used to sign tokens
        // get newest key from alternatives (it's sorted desc)
        var kvp = alternatives.First();
        var keyinput = alternatives[kvp.Key];

        var keybytes = Convert.FromBase64String(keyinput);

        var key = DeriveKey(_tenant, _config, keybytes);
        return new Tuple<Key, int>(key, kvp.Key);
    }

    public string EncodeToken<T>(T token, string prefix, bool contractless = false)
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

        (Key key, int keyId) = GetRandomKey();

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
}