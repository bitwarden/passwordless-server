using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Service.Helpers;
using Service.Models;
using Service.Storage;
using static Fido2NetLib.Fido2;

namespace Service
{
    using Aliases = HashSet<string>;
    public class Fido2ServiceEndpoints
    {
        private IStorage _storage;
        private Fido2 _fido2;
        private string _tenant;
        private readonly ILogger log;
        private readonly IConfiguration config;
        private TokenService _tokenService;

        private Fido2ServiceEndpoints(string tenant, ILogger log, IConfiguration config, IStorage storage)
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

        public static async Task<Fido2ServiceEndpoints> Create(string tenant, ILogger log, IConfiguration config, IStorage storage)
        {
            var instance = new Fido2ServiceEndpoints(tenant, log, config, storage);
            await instance.Init();
            return instance;
        }

        public async Task<ApiResponse<CredentialCreateOptions>> RegisterBegin(FidoRegistrationBeginDTO request)
        {
            var token = _tokenService.DecodeToken<RegisterToken>(request.Token, "register_");
            token.Validate();

            var displayName = token.DisplayName;
            var userId = token.UserId;

            _fido2 = new Fido2(new Fido2Configuration()
            {
                ServerDomain = request.RPID,
                Origins = new HashSet<string>() { request.Origin },
                ServerName = request.ServerName
            });

            try
            {

                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException(nameof(userId));
                }

                var user = new Fido2User()
                {
                    DisplayName = token.DisplayName,
                    Name = token.Username,
                    Id = Encoding.UTF8.GetBytes(userId)
                };

                // 2. Get user existing keys by userid
                var existingKeys = await _storage.GetCredentialsByUserIdAsync(userId); //DemoStorage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

                // 3. Create options
                var authenticatorSelection = new AuthenticatorSelection
                {
                    RequireResidentKey = token.RequireResidentKey,
                    UserVerification = token.UserVerification.ToEnum<UserVerificationRequirement>()
                };

                if (!string.IsNullOrEmpty(token.AuthType))
                    authenticatorSelection.AuthenticatorAttachment = token.AuthType.ToEnum<AuthenticatorAttachment>();

                var options = _fido2.RequestNewCredential(user, existingKeys, authenticatorSelection, token.AttType.ToEnum<AttestationConveyancePreference>());

                // 4. Temporarily store options, session/in-memory cache/redis/db
                //HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());
                var session = _tokenService.EncodeToken(new RegisterSession { Options = options }, null, true);

                // 5. return options to client
                return new ApiResponse<CredentialCreateOptions>() { Data = options, SessionId = session };
            }
            catch (Exception e)
            {
                return new ApiResponse<CredentialCreateOptions>() { Data = new CredentialCreateOptions { Status = "error", ErrorMessage = FormatException(e) } };
            }
        }

        public Task<SignInToken> SignInVerify(SignInVerifyDTO payload)
        {
            var token = _tokenService.DecodeToken<SignInToken>(payload.Token, "verify_");
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

            // cast to RegisterToken to remove Aliases from token
            var token = _tokenService.EncodeToken(tokenProps as RegisterToken, "register_");
            if (tokenProps.Aliases != null)
            {
                await SetAlias(tokenProps.UserId, tokenProps.Aliases);
            }

            return token;
        }

        public async Task<ApiResponse<CredentialMakeResult>> RegisterComplete(RegistrationCompleteDTO request, string deviceInfo, string country)
        {
            try
            {
                var session = _tokenService.DecodeToken<RegisterSession>(request.SessionId, null, true);
                

                _fido2 = new Fido2(new Fido2Configuration()
                {
                    ServerDomain = request.RPID,
                    Origins = new HashSet<string>() { request.Origin },
                    ServerName = request.ServerName
                }
               );

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
                    CredType = success.Result.CredType,
                    CreatedAt = now,
                    LastUsedAt = now,
                    Device = deviceInfo,
                    Country = country,
                    AaGuid = success.Result.Aaguid,
                    RPID = request.RPID,
                    Origin = request.Origin,
                    Nickname = request.Nickname
                });
                
                return new ApiResponse<CredentialMakeResult>() { Data = success };
            }
            catch (Exception e)
            {
                return new ApiResponse<CredentialMakeResult>() { Data = new CredentialMakeResult("error", FormatException(e), null), SessionId = request.SessionId };
            }
        }

        public async Task<ApiResponse<AssertionOptions>> SignInBegin(SignInBeginDTO request)
        {
            _fido2 = new Fido2(new Fido2Configuration()
            {
                ServerDomain = request.RPID,
                Origins = new HashSet<string>() { request.Origin },
                ServerName = request.ServerName
            });

            try
            {
                var existingCredentials = new List<PublicKeyCredentialDescriptor>();
                var userId = request.UserId;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get registered credentials from database
                    existingCredentials = await _storage.GetCredentialsByUserIdAsync(userId);
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
                            await _storage.StoreAlias(useridbytes, new Aliases(1) { hashedAlias });
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

                var id = _tokenService.EncodeToken(options, null, true);

                // Return options to client
                return new ApiResponse<AssertionOptions> { Data = options, SessionId = id };
            }

            catch (Exception e)
            {
                return new ApiResponse<AssertionOptions>(new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) });
            }
        }

        public async Task<ApiResponse<string>> SignInComplete(SignInCompleteDTO request, string device, string country)
        {
            _fido2 = new Fido2(new Fido2Configuration()
            {
                ServerDomain = request.RPID,
                Origins = new HashSet<string>() { request.Origin },
                ServerName = request.ServerName
            }
              );

            // Get the assertion options we sent the client
            var options = _tokenService.DecodeToken<AssertionOptions>(request.SessionId, null, true);

            // Get registered credential from database
            var creds = await _storage.GetCredential(request.Response.Id);

            if (creds == null)
            {
                throw new ApiException("Unknown credentials: We don't recognize the credentials you sent us.", 400);
            }

            // Get credential counter from database
            var storedCounter = creds.SignatureCounter;

            // Create callback to check if userhandle owns the credentialId
            IsUserHandleOwnerOfCredentialIdAsync callback = (args, token) =>
            {
                return Task.FromResult(creds.UserHandle == args.UserHandle);
            };

            // Make the assertion
            var res = await _fido2.MakeAssertionAsync(request.Response, options, creds.PublicKey, storedCounter, callback);

            // Store the updated counter
            await _storage.UpdateCredential(res.CredentialId, res.Counter, country, device);

            var tokendata = new SignInToken
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
                ExpiresAt = DateTime.UtcNow.AddSeconds(120)
            };

            var token = _tokenService.EncodeToken(tokendata, "verify_");

            // return OK to client
            return new ApiResponse<string> { Data = token };
        }

        public void ValidateAliases(Aliases aliases, bool throwifnull = false)
        {
            try
            {
                if (throwifnull && aliases == null) { throw new ArgumentException("Aliases was null"); }
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

        public Task SetAlias(string userId, Aliases aliases)
        {
            ValidateUserId(userId);
            return SetAlias(Encoding.UTF8.GetBytes(userId), aliases);
        }

        public async Task SetAlias(byte[] userId, Aliases aliases)
        {
            if (userId == null || userId.Length == 0) { throw new ApiException("userId most not be null", 400); }
            ValidateAliases(aliases, true);
            var values = new Aliases(aliases.Count);
            foreach (var alias in aliases)
            {
                values.Add(HashAlias(alias, _tenant));
            }

            await _storage.StoreAlias(userId, values);
        }

        private void ValidateUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ApiException("userId must not be null or empty", 400);
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
            string environmentsalt = config["SALT_ALIAS"];
            if (string.IsNullOrEmpty(environmentsalt)) throw new Exception("SALT_ALIAS environment variable is missing");

            var salt = SHA256.HashData(Encoding.UTF8.GetBytes(tenant + environmentsalt));

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

        private string FormatException(Exception e)
        {
            return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
        }

    }
}
