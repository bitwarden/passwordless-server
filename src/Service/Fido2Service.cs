using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;
using Passwordless.Service.Validation;

namespace Passwordless.Service;

using Aliases = HashSet<string>;

public class Fido2Service : IFido2Service
{
    private readonly ITenantStorage _storage;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger _log;
    private readonly ITokenService _tokenService;
    private readonly IEventLogger _eventLogger;
    private readonly IFeatureContextProvider _featureContextProvider;
    private readonly IMetadataService _metadataService;
    private readonly TimeProvider _timeProvider;

    public Fido2Service(ITenantProvider tenantProvider,
        ILogger log,
        ITenantStorage storage,
        ITokenService tokenService,
        IEventLogger eventLogger,
        IFeatureContextProvider featureContextProvider,
        IMetadataService metadataService,
        TimeProvider timeProvider)
    {
        _storage = storage;
        _tenantProvider = tenantProvider;
        _log = log;
        _tokenService = tokenService;
        _eventLogger = eventLogger;
        _featureContextProvider = featureContextProvider;
        _metadataService = metadataService;
        _timeProvider = timeProvider;
    }

    public async Task<string> CreateRegisterToken(RegisterToken tokenProps)
    {
        if (tokenProps.ExpiresAt == default)
        {
            tokenProps.ExpiresAt = _timeProvider.GetUtcNow().DateTime.AddSeconds(120);
        }

        ValidateAliases(tokenProps.Aliases);
        ValidateUserId(tokenProps.UserId);
        ValidateUsername(tokenProps.Username);

        var features = await _featureContextProvider.UseContext();
        if (features.MaxUsers.HasValue)
        {
            var credentials = await _storage.GetCredentialsByUserIdAsync(tokenProps.UserId);
            if (!credentials.Any())
            {
                var users = await _storage.GetUsersCount();
                if (users >= features.MaxUsers)
                {
                    throw new ApiException("max_users_reached", "Maximum number of users reached", 400);
                }
            }
        }

        // Attestation
        if (string.IsNullOrEmpty(tokenProps.Attestation)) tokenProps.Attestation = "none";
        RegisterTokenValidator.ValidateAttestation(tokenProps, features);

        // check if aliases is available
        if (tokenProps.Aliases != null)
        {
            ValidateAliases(tokenProps.Aliases);

            var hashedAliases = tokenProps.Aliases.Select(alias => HashAlias(alias, _tenantProvider.Tenant));

            // todo: check if alias exists and belongs to different user.
            var isAvailable = await _storage.CheckIfAliasIsAvailable(hashedAliases, tokenProps.UserId);
            if (!isAvailable)
            {
                throw new ApiException("alias_conflict", "Alias is already in use by another userid", 409);
            }
        }

        var token = await _tokenService.EncodeTokenAsync(tokenProps, "register_");

        _eventLogger.LogRegistrationTokenCreatedEvent(tokenProps.UserId);

        return token;
    }

    public async Task<SessionResponse<CredentialCreateOptions>> RegisterBegin(FidoRegistrationBeginDTO request)
    {
        var features = await _featureContextProvider.UseContext();

        var token = await _tokenService.DecodeTokenAsync<RegisterToken>(request.Token, "register_");
        token.Validate(_timeProvider);

        var userId = token.UserId;

        var fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string> { request.Origin },
            ServerName = request.RPID,
            MDSCacheDirPath = ".mds-cache"
        }, _metadataService);

        if (string.IsNullOrEmpty(userId))
        {
            throw new ApiException("missing_userid", "The token does not contain a valid userId", 400);
        }

        var user = new Fido2User
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
                ResidentKey = token.Discoverable ? ResidentKeyRequirement.Required : ResidentKeyRequirement.Discouraged,
                UserVerification = token.UserVerification.ToEnum<UserVerificationRequirement>(),
                AuthenticatorAttachment = token.AuthenticatorType?.ToEnum<AuthenticatorAttachment>()
            };

            // Attestation
            if (string.IsNullOrEmpty(token.Attestation)) token.Attestation = "none";
            RegisterTokenValidator.ValidateAttestation(token, features);

            var attestation = token.Attestation.ToEnum<AttestationConveyancePreference>();

            var options = fido2.RequestNewCredential(
                user,
                keyIds,
                authenticatorSelection,
                attestation,
                new AuthenticationExtensionsClientInputs
                {
                    CredProps = true
                });

            var session = await _tokenService.EncodeTokenAsync(new RegisterSession { Options = options, Aliases = token.Aliases, AliasHashing = token.AliasHashing }, "session_", true);

            _eventLogger.LogRegistrationBeganEvent(userId);

            // return options to client
            return new SessionResponse<CredentialCreateOptions> { Data = options, Session = session };
        }
        catch (ArgumentException e)
        {
            throw new ApiException("invalid_argument", e.Message, 400);
        }
    }

    public async Task<TokenResponse> RegisterComplete(RegistrationCompleteDTO request, string deviceInfo, string country)
    {
        var session = await _tokenService.DecodeTokenAsync<RegisterSession>(request.Session, "session_", true);

        var fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string> { request.Origin },
            ServerName = request.RPID,
            MDSCacheDirPath = ".mds-cache"
        }, _metadataService);

        var success = await fido2.MakeNewCredentialAsync(request.Response, session.Options, async (args, _) =>
        {
            bool exists = await _storage.ExistsAsync(args.CredentialId);
            return !exists;
        });

        // Check whether we're allowed to register credentials for this autheneticator
        var features = await _featureContextProvider.UseContext();
        if (features.AllowAttestation)
        {
            var configuredAuthenticators = await _storage.GetAuthenticatorsAsync();
            var blacklist = configuredAuthenticators.Where(x => !x.IsAllowed).ToImmutableList();
            if (blacklist.Any() && blacklist.Any(x => x.AaGuid == success.Result!.AaGuid))
            {
                throw new ApiException("authenticator_not_allowed", "The authenticator is on the blocklist and is not allowed to be used for registration.", 400);
            }

            var whitelist = configuredAuthenticators.Where(x => x.IsAllowed).ToImmutableList();
            if (whitelist.Any() && whitelist.All(x => x.AaGuid != success.Result!.AaGuid))
            {
                if (session.Options.Attestation == AttestationConveyancePreference.None)
                {
                    throw new ApiException("attestation_required", "Attestation 'none' was used for registration, but an allowlist was configured. Please use a supported attestation method.", 400);
                }
                throw new ApiException("authenticator_not_allowed", "An allowlist was configured. The authenticator is not found on the allowlist and is not allowed to be used for registration.", 400);
            }
        }

        var userId = Encoding.UTF8.GetString(success.Result.User.Id);

        // Add aliases
        try
        {
            if (session.Aliases != null && session.Aliases.Any())
            {
                await SetAlias(new AliasPayload
                {
                    Aliases = session.Aliases,
                    Hashing = session.AliasHashing,
                    UserId = userId
                });
            }
        }
        catch (Exception e)
        {
            _log.LogError(e, "Error while saving alias during /register/complete");
            throw;
        }

        var now = _timeProvider.GetUtcNow().DateTime;
        var descriptor = new PublicKeyCredentialDescriptor(success.Result.Id);

        await _storage.AddCredentialToUser(session.Options.User, new StoredCredential
        {
            Descriptor = descriptor,
            PublicKey = success.Result.PublicKey,
            UserHandle = success.Result.User.Id,
            SignatureCounter = success.Result.SignCount,
            AttestationFmt = success.Result.AttestationFormat,
            CreatedAt = now,
            LastUsedAt = now,
            Device = deviceInfo,
            Country = country,
            AaGuid = success.Result.AaGuid,
            RPID = request.RPID,
            Origin = request.Origin,
            Nickname = request.Nickname,
            BackupState = success.Result.IsBackedUp,
            IsBackupEligible = success.Result.IsBackupEligible,
            IsDiscoverable = request.Response.Extensions.CredProps?.Rk,
        });

        var tokenData = new VerifySignInToken
        {
            UserId = userId,
            Success = true,
            Origin = request.Origin,
            RpId = session.Options.Rp.Id,
            Timestamp = _timeProvider.GetUtcNow().DateTime,
            CredentialId = success.Result.Id,
            Device = deviceInfo,
            Country = country,
            Nickname = request.Nickname,
            ExpiresAt = _timeProvider.GetUtcNow().DateTime.AddSeconds(120),
            TokenId = Guid.NewGuid(),
            Type = "passkey_register"
        };

        _eventLogger.LogRegistrationCompletedEvent(userId);

        var token = await _tokenService.EncodeTokenAsync(tokenData, "verify_");

        return new TokenResponse(token);
    }

    public async Task<string> CreateSigninToken(SigninTokenRequest request)
    {
        ValidateUserId(request.UserId);

        _eventLogger.LogSigninTokenCreatedEvent(request.UserId);

        var tokenProps = new VerifySignInToken
        {
            Success = true,
            UserId = request.UserId,
            Timestamp = _timeProvider.GetUtcNow().DateTime,
            ExpiresAt = _timeProvider.GetUtcNow().DateTime.AddSeconds(120),
            TokenId = Guid.NewGuid(),
            Type = "generated_signin",
            RpId = request.RPID,
            Origin = request.Origin
        };

        return await _tokenService.EncodeTokenAsync(tokenProps, "verify_");
    }

    public async Task<SessionResponse<AssertionOptions>> SignInBegin(SignInBeginDTO request)
    {
        var fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string> { request.Origin },
            ServerName = request.RPID,
            MDSCacheDirPath = ".mds-cache"
        }, _metadataService);

        var existingCredentials = new List<PublicKeyCredentialDescriptor>();
        var userId = request.UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            // Get registered credentials from database
            existingCredentials = (await _storage.GetCredentialsByUserIdAsync(userId)).Select(c => c.Descriptor).ToList();
            _log.LogInformation("event=signin/begin account={account} arg={arg}", _tenantProvider, "userid");
        }
        else if (!string.IsNullOrEmpty(request.Alias))
        {
            var hashedAlias = HashAlias(request.Alias, _tenantProvider.Tenant);

            existingCredentials = await _storage.GetCredentialsByAliasAsync(hashedAlias);
            _log.LogInformation("event=signin/begin account={account} arg={arg} foundCredentials={foundCredentials}", _tenantProvider, "alias", existingCredentials.Count);
        }
        else
        {
            _log.LogInformation("event=signin/begin account={account} arg={arg}", _tenantProvider, "empty");
        }

        // Create options
        var uv = string.IsNullOrEmpty(request.UserVerification) ? UserVerificationRequirement.Preferred : request.UserVerification.ToEnum<UserVerificationRequirement>();
        var options = fido2.GetAssertionOptions(
            existingCredentials,
            uv
        );

        var session = await _tokenService.EncodeTokenAsync(options, "session_", true);

        // Return options to client
        return new SessionResponse<AssertionOptions> { Data = options, Session = session };
    }

    public async Task<TokenResponse> SignInComplete(SignInCompleteDTO request, string device, string country)
    {
        var fido2 = new Fido2(new Fido2Configuration
        {
            ServerDomain = request.RPID,
            Origins = new HashSet<string> { request.Origin },
            ServerName = request.RPID,
            MDSCacheDirPath = ".mds-cache"
        }, _metadataService);

        // Get the assertion options we sent the client
        var options = await _tokenService.DecodeTokenAsync<AssertionOptions>(request.Session, "session_", true);

        // Get registered credential from database
        var credential = await _storage.GetCredential(request.Response.Id);
        if (credential == null)
        {
            throw new UnknownCredentialException(Base64Url.Encode(request.Response.Id));
        }

        // Get credential counter from database
        var storedCounter = credential.SignatureCounter;

        // Create callback to check if userhandle owns the credentialId
        IsUserHandleOwnerOfCredentialIdAsync callback = (args, token) => Task.FromResult(credential.UserHandle.SequenceEqual(args.UserHandle));

        // Make the assertion
        var storedCredentials = (await _storage.GetCredentialsByUserIdAsync(request.Session)).Select(c => c.PublicKey).ToList();
        var res = await fido2.MakeAssertionAsync(request.Response, options, credential.PublicKey, storedCredentials, storedCounter, callback);

        // Store the updated counter
        await _storage.UpdateCredential(res.CredentialId, res.SignCount, country, device);

        var userId = Encoding.UTF8.GetString(credential.UserHandle);

        var tokenData = new VerifySignInToken
        {
            UserId = userId,
            Success = true,
            Origin = request.Origin,
            RpId = request.RPID,
            Timestamp = _timeProvider.GetUtcNow().DateTime,
            Device = device,
            Country = country,
            Nickname = credential.Nickname,
            CredentialId = credential.Descriptor.Id,
            ExpiresAt = _timeProvider.GetUtcNow().DateTime.AddSeconds(120),
            TokenId = Guid.NewGuid(),
            Type = "passkey_signin"
        };

        _eventLogger.LogUserSignInCompletedEvent(userId);

        var token = await _tokenService.EncodeTokenAsync(tokenData, "verify_");

        // return OK to client
        return new TokenResponse(token);
    }

    public async Task<VerifySignInToken> SignInVerify(SignInVerifyDTO payload)
    {
        var token = await _tokenService.DecodeTokenAsync<VerifySignInToken>(payload.Token, "verify_");
        token.Validate(_timeProvider);
        _eventLogger.LogUserSignInTokenVerifiedEvent(token.UserId);

        return token;
    }

    private static void ValidateAliases(Aliases aliases, bool throwIfNull = false)
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
            values.Add(HashAlias(alias, _tenantProvider.Tenant), plaintext);
        }

        try
        {
            await _storage.StoreAlias(data.UserId, values);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException { Number: 2627 })
            {
                throw new ApiException("alias_conflict", "Alias is already in use by another userid", 409);
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
        _log.LogInformation("SHA256 Hashing username took {duration}ms", sw.ElapsedMilliseconds);

        return hashedUsername;
    }
}