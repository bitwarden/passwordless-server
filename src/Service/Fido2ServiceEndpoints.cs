using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

using Aliases = HashSet<string>;
public class Fido2ServiceEndpoints
{
    private readonly ITenantStorage _storage;
    private Fido2 _fido2;
    private readonly string _tenant;
    private readonly ILogger log;
    private readonly IConfiguration config;
    private readonly TokenService _tokenService;

    private Fido2ServiceEndpoints(string tenant, ILogger log, IConfiguration config, ITenantStorage storage)
    {
        _storage = storage;
        _tenant = tenant;
        this.log = log;
        this.config = config;
        _tokenService = new TokenService(tenant, log, config, _storage);
    }

    private async Task Init()
    {
        await _tokenService.Init();
    }

    public static async Task<Fido2ServiceEndpoints> Create(string tenant, ILogger log, IConfiguration config, ITenantStorage storage)
    {
        var instance = new Fido2ServiceEndpoints(tenant, log, config, storage);
        await instance.Init();
        return instance;
    }

    public async Task<SessionResponse<CredentialCreateOptions>> RegisterBegin(FidoRegistrationBeginDTO request)
    {
        var token = _tokenService.DecodeToken<RegisterToken>(request.Token, "register_");
        token.Validate();

        var userId = token.UserId;

        _fido2 = new Fido2(new Fido2Configuration()
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string>() { request.Origin },
            ServerName = request.ServerName
        });

        if (string.IsNullOrEmpty(userId))
        {
            throw new ApiException("missing_userid", "The token does not contain a valid userId", 400);
        }

        var user = new Fido2User()
        {
            DisplayName = token.DisplayName ?? token.Username,
            Name = token.Username,
            Id = Encoding.UTF8.GetBytes(userId)
        };

        //  Get user existing keys by userid
        var existingKeys = await _storage.GetCredentialsByUserIdAsync(userId); //DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

        var keyIds = existingKeys.Select(k => k.Descriptor).ToList();

        try
        {
            // Clean up default values
            if (string.IsNullOrEmpty(token.UserVerification)) token.UserVerification = "preferred";
            if (token.AuthenticatorType?.ToLower() == "any") token.AuthenticatorType = null;

            // Selection
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = token.Discoverable,
                UserVerification = token.UserVerification.ToEnum<UserVerificationRequirement>(),
                AuthenticatorAttachment = token.AuthenticatorType?.ToEnum<AuthenticatorAttachment>()
            };

            // Attestation
            if (string.IsNullOrEmpty(token.Attestation)) token.Attestation = "none";
            if (token.Attestation.ToLower() != "none")
            {
                throw new ApiException("invalid_attestation", "Attestation type not supported", 400);
            }

            var attestation = token.Attestation.ToEnum<AttestationConveyancePreference>();

            var options = _fido2.RequestNewCredential(user, keyIds, authenticatorSelection, attestation);

            var session = _tokenService.EncodeToken(new RegisterSession { Options = options }, "session_", true);

            // return options to client
            return new SessionResponse<CredentialCreateOptions>() { Data = options, Session = session };
        }
        catch (ArgumentException e)
        {
            throw new ApiException("invalid_argument", e.Message, 400);
        }
    }

    public Task<VerifySignInToken> SignInVerify(SignInVerifyDTO payload)
    {
        var token = _tokenService.DecodeToken<VerifySignInToken>(payload.Token, "verify_");
        return Task.FromResult(token);
    }

    public async Task<string> CreateToken(RegisterTokenDTO tokenProps)
    {
        if (tokenProps.ExpiresAt == default)
        {
            tokenProps.ExpiresAt = DateTime.UtcNow.AddSeconds(120);
        }

        ValidateAliases(tokenProps.Aliases);
        ValidateUserId(tokenProps.UserId);
        ValidateUsername(tokenProps.Username);

        // Attestation
        if (string.IsNullOrEmpty(tokenProps.Attestation)) tokenProps.Attestation = "none";
        if (tokenProps.Attestation.ToLower() != "none")
        {
            throw new ApiException("invalid_attestation", "Attestation type not supported", 400);
        }

        // cast to RegisterToken to remove Aliases from token
        var token = _tokenService.EncodeToken(tokenProps as RegisterToken, "register_");
        if (tokenProps.Aliases != null)
        {
            var aliasPayload = new AliasPayload() { Aliases = tokenProps.Aliases, UserId = tokenProps.UserId, Hashing = tokenProps.AliasHashing };
            await SetAlias(aliasPayload);
        }

        return token;
    }

    public async Task<TokenResponse> RegisterComplete(RegistrationCompleteDTO request, string deviceInfo, string country)
    {
        var session = _tokenService.DecodeToken<RegisterSession>(request.Session, "session_", true);

        _fido2 = new Fido2(new Fido2Configuration()
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string>() { request.Origin },
            ServerName = request.ServerName
        });

        // Create callback so that lib can verify credential id is unique to this user
        IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, token) =>
        {
            bool exists = await _storage.ExistsAsync(args.CredentialId);
            return !exists;
        };

        var success = await _fido2.MakeNewCredentialAsync(request.Response, session.Options, callback);

        var now = DateTime.UtcNow;

        var descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId);
        await _storage.AddCredentialToUser(session.Options.User, new StoredCredential
        {
            Descriptor = descriptor,
            PublicKey = success.Result.PublicKey,
            UserHandle = success.Result.User.Id,
            SignatureCounter = success.Result.Counter,
            AttestationFmt = success.Result.CredType,
            CreatedAt = now,
            LastUsedAt = now,
            Device = deviceInfo,
            Country = country,
            AaGuid = success.Result.Aaguid,
            RPID = request.RPID,
            Origin = request.Origin,
            Nickname = request.Nickname
        });

        var tokenData = new VerifySignInToken()
        {
            UserId = Encoding.UTF8.GetString(success.Result.User.Id),
            Success = true,
            Origin = request.Origin,
            RPID = session.Options.Rp.Id,
            Timestamp = DateTime.UtcNow,
            CredentialId = success.Result.CredentialId,
            Device = deviceInfo,
            Country = country,
            Nickname = request.Nickname,
            ExpiresAt = DateTime.UtcNow.AddSeconds(120),
            TokenId = Guid.NewGuid(),
            Type = "passkey_register"
        };

        var token = _tokenService.EncodeToken(tokenData, "verify_");

        return new TokenResponse(token);
    }

    public async Task<SessionResponse<AssertionOptions>> SignInBegin(SignInBeginDTO request)
    {
        _fido2 = new Fido2(new Fido2Configuration()
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string>() { request.Origin },
            ServerName = request.ServerName
        });

        var existingCredentials = new List<PublicKeyCredentialDescriptor>();
        var userId = request.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            // Get registered credentials from database
            existingCredentials = (await _storage.GetCredentialsByUserIdAsync(userId)).Select(c => c.Descriptor).ToList();
            log.LogInformation("event=signin/begin account={account} arg={arg}", _tenant, "userid");
        }
        else if (!string.IsNullOrEmpty(request.Alias))
        {
            var hashedAlias = HashAlias(request.Alias, _tenant);

            existingCredentials = await _storage.GetCredentialsByAliasAsync(hashedAlias);
            log.LogInformation("event=signin/begin account={account} arg={arg} foundCredentials={foundCredentials}", _tenant, "alias", existingCredentials.Count);


            if (existingCredentials.Count == 0)
            {
                // If we couldn't find any aliases, try to find them using legacy hashing
                var legacyHashed = LegacyHashAlias(request.Alias, _tenant);

                existingCredentials = await _storage.GetCredentialsByAliasAsync(legacyHashed);
                log.LogInformation("event=signin/begin account={account} arg={arg} foundCredentials={foundCredentials}", _tenant, "alias", existingCredentials.Count);

                if (existingCredentials.Count > 0)
                {
                    // If we found credentials using legacy hashing, Migrate to new hashing scheme
                    string aliasUserIdBase64 = await _storage.GetUserIdByAliasAsync(legacyHashed);
                    var useridbytes = Convert.FromBase64String(aliasUserIdBase64);
                    var str = Encoding.UTF8.GetString(useridbytes);
                    await _storage.StoreAlias(str,
                        new Dictionary<string, string>()
                        {
                            {hashedAlias,null}
                        });
                }
            }
        }
        else
        {
            log.LogInformation("event=signin/begin account={account} arg={arg}", _tenant, "empty");
        }

        // Create options
        var uv = string.IsNullOrEmpty(request.UserVerification) ? UserVerificationRequirement.Discouraged : request.UserVerification.ToEnum<UserVerificationRequirement>();
        var options = _fido2.GetAssertionOptions(
            existingCredentials,
            uv
        );

        var session = _tokenService.EncodeToken(options, "session_", true);

        // Return options to client
        return new SessionResponse<AssertionOptions> { Data = options, Session = session };
    }

    public async Task<TokenResponse> SignInComplete(SignInCompleteDTO request, string device, string country)
    {
        _fido2 = new Fido2(new Fido2Configuration()
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string>() { request.Origin },
            ServerName = request.ServerName
        });

        // Get the assertion options we sent the client
        var options = _tokenService.DecodeToken<AssertionOptions>(request.Session, "session_", true);

        // Get registered credential from database
        var creds = await _storage.GetCredential(request.Response.Id);
        if (creds == null)
        {
            throw new UnknownCredentialException(Base64Url.Encode(request.Response.Id));
        }

        // Get credential counter from database
        var storedCounter = creds.SignatureCounter;

        // Create callback to check if userhandle owns the credentialId
        IsUserHandleOwnerOfCredentialIdAsync callback = (args, token) => Task.FromResult(creds.UserHandle.SequenceEqual(args.UserHandle));

        // Make the assertion
        var res = await _fido2.MakeAssertionAsync(request.Response, options, creds.PublicKey, storedCounter, callback);

        // Store the updated counter
        await _storage.UpdateCredential(res.CredentialId, res.Counter, country, device);

        var tokenData = new VerifySignInToken
        {
            UserId = Encoding.UTF8.GetString(creds.UserHandle),
            Success = true,
            Origin = request.Origin,
            RPID = request.RPID,
            Timestamp = DateTime.UtcNow,
            Device = device,
            Country = country,
            Nickname = creds.Nickname,
            CredentialId = creds.Descriptor.Id,
            ExpiresAt = DateTime.UtcNow.AddSeconds(120),
            TokenId = Guid.NewGuid(),
            Type = "passkey_signin"
        };

        var token = _tokenService.EncodeToken(tokenData, "verify_");

        // return OK to client
        return new TokenResponse(token);
    }

    public void ValidateAliases(Aliases aliases, bool throwIfNull = false)
    {
        try
        {
            if (throwIfNull && aliases == null) { throw new ArgumentException("Aliases was null"); }
            if (aliases == null) return;
            if (aliases.Count > 10) { throw new ArgumentException("Too many aliases, maximum is 10"); }
            foreach (var alias in aliases)
            {
                if (string.IsNullOrWhiteSpace(alias)) { throw new ArgumentException("Alias must be a non empty/whitepsace string"); }
                if (alias.Length > 250) { throw new ArgumentException("Alias was is too long, maximum is 250"); }
            }
        }
        catch (ArgumentException ex)
        {
            throw new ApiException("Alias validation failed: " + ex.Message, 400);
        }
    }

    //public Task SetAlias(string userId, Aliases aliases)
    //{
    //    ValidateUserId(userId);
    //    return SetAlias(Encoding.UTF8.GetBytes(userId), aliases);
    //}

    //public async Task SetAlias(byte[] userId, Aliases aliases)
    //{
    //    if (userId == null || userId.Length == 0) { throw new ApiException("userId most not be null", 400); }
    //    ValidateAliases(aliases, true);
    //    var values = new Aliases(aliases.Count);
    //    foreach (var alias in aliases)
    //    {
    //        values.Add(HashAlias(alias, _tenant));
    //    }

    //    await _storage.StoreAlias(userId, values);
    //}

    public Task<List<AliasPointer>> GetAliases(string userId)
    {
        return _storage.GetAliasesByUserId(userId);
    }

    public async Task SetAlias(AliasPayload data)
    {
        if (data.UserId.IsNullOrEmpty())
        {
            throw new ApiException("userId most not be null", 400);
        }

        ValidateAliases(data.Aliases);

        var values = new Dictionary<string, string>();
        foreach (var alias in data.Aliases)
        {
            string plaintext = null;
            if (data.Hashing == false)
            {
                plaintext = alias;
            }
            values.Add(HashAlias(alias, _tenant), plaintext);
        }

        try
        {
            await _storage.StoreAlias(data.UserId, values);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException { Number: 2627 })
            {
                throw new ApiException("alias_conflict", "Alias is already in use", 409);
            }
        }
    }

    private void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ApiException("Invalid UserId: UserId cannot be null or empty", 400);
        }
    }

    private void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ApiException("Invalid Username: Username cannot be null or empty", 400);
        }
    }

    private string HashAlias(string username, string tenant)
    {
        var sw = Stopwatch.StartNew();
        var hashedUsername = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(tenant + username)));
        sw.Stop();
        log.LogInformation("SHA256 Hashing username took {duration}ms", sw.ElapsedMilliseconds);

        return hashedUsername;
    }

    private string LegacyHashAlias(string username, string tenant)
    {
        var sw = Stopwatch.StartNew();
        string environmentSalt = config["SALT_ALIAS"];
        if (string.IsNullOrEmpty(environmentSalt)) throw new Exception("SALT_ALIAS environment variable is missing");

        var salt = SHA256.HashData(Encoding.UTF8.GetBytes(tenant + environmentSalt));

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: username,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        sw.Stop();
        log.LogInformation("Hashing username took {duration}ms", sw.ElapsedMilliseconds);
        return hashed;
    }
}