using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Constants;
using Passwordless.Common.Extensions;
using Passwordless.Common.Models;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Utils;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Extensions.Models;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public record AppDeletionResult(string Message, bool IsDeleted, DateTime? DeleteAt, IReadOnlyCollection<string> AdminEmails);

public interface ISharedManagementService
{
    Task<bool> IsAvailable(string appId);
    Task<CreateAppResultDto> GenerateAccount(string appId, CreateAppDto options);
    Task<ValidateSecretKeyDto> ValidateSecretKey(string secretKey);
    Task<ValidatePublicKeyDto> ValidatePublicKey(string publicKey);
    Task FreezeAccount(string accountName);
    Task UnFreezeAccount(string accountName);
    Task<AppDeletionResult> DeleteApplicationAsync(string appId);
    Task<AppDeletionResult> MarkDeleteApplicationAsync(string appId, string deletedBy, string baseUrl);
    Task<IEnumerable<string>> GetApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(string appId, ManageFeaturesRequest payload);
    Task<AppFeatureResponse> GetFeaturesAsync(string appId);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreatePublicKeyRequest payload);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreateSecretKeyRequest payload);
    Task<IReadOnlyCollection<ApiKeyResponse>> ListApiKeysAsync(string appId);
    Task LockApiKeyAsync(string appId, string apiKeyId);
    Task UnlockApiKeyAsync(string appId, string apiKeyId);
    Task DeleteApiKeyAsync(string appId, string apiKeyId);
    Task EnableGenerateSignInTokenEndpoint(string appId);
    Task DisableGenerateSignInTokenEndpoint(string appId);
}

public class SharedManagementService : ISharedManagementService
{
    private readonly ILogger _logger;
    private readonly IEventLogger _eventLogger;
    private readonly IConfiguration config;
    private readonly ISystemClock _systemClock;
    private readonly ITenantStorageFactory tenantFactory;
    private readonly IGlobalStorageFactory _globalStorageFactory;

    public SharedManagementService(ITenantStorageFactory tenantFactory,
        IGlobalStorageFactory globalStorageFactory,
        IConfiguration config,
        ISystemClock systemClock,
        ILogger<SharedManagementService> logger,
        IEventLogger eventLogger)
    {
        this.tenantFactory = tenantFactory;
        _globalStorageFactory = globalStorageFactory;
        this.config = config;
        _systemClock = systemClock;
        _logger = logger;
        _eventLogger = eventLogger;
    }


    public async Task<bool> IsAvailable(string accountName)
    {
        // check if tenant already exists
        var storage = tenantFactory.Create(accountName);
        return !await storage.TenantExists();
    }

    public async Task<CreateAppResultDto> GenerateAccount(string appId, CreateAppDto options)
    {
        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new ApiException($"'{nameof(appId)}' cannot be null, empty or whitespace.", 400);
        }
        if (options == null)
        {
            throw new ApiException("No application creation options have been defined.", 400);
        }

        var accountName = appId;
        var adminEmail = options.AdminEmail;

        if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(accountName))
        {
            throw new ApiException("Please set argument 'accountName' and 'adminEmail'", 400);
        }

        var regi = new Regex("^[A-Za-z][A-Za-z0-9]{2,62}$");
        var isValid = regi.IsMatch(accountName);

        if (!isValid)
        {
            throw new ApiException("accountName needs to be alphanumeric and start with a letter", 400);
        }

        ITenantStorage storage = tenantFactory.Create(accountName);

        if (await storage.TenantExists())
        {
            throw new ApiException($"accountName '{accountName}' is not available", 409);
        }

        string apiKey1 = await SetupApiKey(accountName, storage);
        string apiKey2 = await SetupApiKey(accountName, storage);

        (string original, string hashed) apiSecret1 = await SetupApiSecret(accountName, storage);
        (string original, string hashed) apiSecret2 = await SetupApiSecret(accountName, storage);

        var account = new AccountMetaInformation
        {
            AcountName = accountName,
            AdminEmails = new[] { adminEmail },
            CreatedAt = DateTime.UtcNow,
            SubscriptionTier = "Free",
            Features = new AppFeature
            {
                Tenant = accountName,
                EventLoggingIsEnabled = options.EventLoggingIsEnabled,
                EventLoggingRetentionPeriod = options.EventLoggingRetentionPeriod,
                MaxUsers = options.MaxUsers,
                IsGenerateSignInTokenEndpointEnabled = true
            }
        };
        await storage.SaveAccountInformation(account);
        return new CreateAppResultDto
        {
            ApiKey1 = apiKey1,
            ApiKey2 = apiKey2,
            ApiSecret1 = apiSecret1.original,
            ApiSecret2 = apiSecret2.original,
            Message = "Store keys safely. They will only be shown to you once."
        };
    }

    public async Task<ValidateSecretKeyDto> ValidateSecretKey(string secretKey)
    {
        var appId = GetAppId(secretKey);
        var storage = tenantFactory.Create(appId);

        var existingKey = await storage.GetApiKeyAsync(secretKey);
        if (existingKey != null)
        {
            if (existingKey.IsLocked)
            {
                _eventLogger.LogDisabledApiKeyUsedEvent(_systemClock.UtcNow.UtcDateTime, appId, new ApplicationSecretKey(secretKey));
                throw new ApiException("ApiKey has been disabled due to account deletion in process. Please see email to reverse.", 403);
            }

            if (ApiKeyUtils.Validate(existingKey.ApiKey, secretKey))
            {
                return new ValidateSecretKeyDto(appId, existingKey.Scopes);
            }
        }

        _eventLogger.LogInvalidApiSecretUsedEvent(_systemClock.UtcNow.UtcDateTime, appId, new ApplicationSecretKey(secretKey));
        _logger.LogInformation("ApiSecret was not valid. {AppId} {ApiKey}", appId, secretKey?[..20]);
        throw new ApiException("ApiSecret was not valid", 401);
    }

    public async Task<ValidatePublicKeyDto> ValidatePublicKey(string publicKey)
    {
        var appId = GetAppId(publicKey);
        var storage = tenantFactory.Create(appId);

        var existingKey = await storage.GetApiKeyAsync(publicKey);
        if (existingKey != null && existingKey.ApiKey == publicKey)
        {
            if (!existingKey.IsLocked)
            {
                return new ValidatePublicKeyDto(appId, existingKey.Scopes);
            }

            _eventLogger.LogDisabledPublicKeyUsedEvent(_systemClock.UtcNow.UtcDateTime, appId, new ApplicationPublicKey(publicKey));
            throw new ApiException("ApiKey has been disabled due to account deletion in process. Please see email to reverse.", 403);
        }

        _eventLogger.LogInvalidPublicKeyUsedEvent(_systemClock.UtcNow.UtcDateTime, appId, new ApplicationPublicKey(publicKey));
        _logger.LogWarning("Apikey was not valid. {AppId} {ApiKey}", appId, publicKey);
        throw new ApiException("Apikey was not valid", 401);
    }

    private string GetAppId(string apiKey)
    {
        try
        {
            return ApiKeyUtils.GetAppId(apiKey);
        }
        catch (Exception)
        {
            _logger.LogError("Could not parse accountname={apikey}", apiKey);
            throw new ApiException("Please supply the apikey or apisecret header with correct value.", 401);
        }
    }

    public async Task FreezeAccount(string accountName)
    {
        var storage = tenantFactory.Create(accountName);
        await storage.LockAllApiKeys(true);
    }

    public async Task UnFreezeAccount(string accountName)
    {
        var storage = tenantFactory.Create(accountName);
        await storage.LockAllApiKeys(false);
        await storage.SetAppDeletionDate(null);
    }

    public async Task<AppDeletionResult> DeleteApplicationAsync(string appId)
    {
        var storage = tenantFactory.Create(appId);
        var accountInformation = await storage.GetAccountInformation();

        if (accountInformation == null)
        {
            throw new ApiException("app_not_found", "App was not found.", 400);
        }

        if (!accountInformation.DeleteAt.HasValue || accountInformation.DeleteAt > _systemClock.UtcNow)
        {
            throw new ApiException("app_not_pending_deletion", "App was not scheduled for deletion.", 400);
        }

        await storage.DeleteAccount();

        return new AppDeletionResult(
            $"The app '{accountInformation.AcountName}' was deleted.",
            true,
            _systemClock.UtcNow.UtcDateTime,
            accountInformation.AdminEmails);
    }

    public async Task<AppDeletionResult> MarkDeleteApplicationAsync(string appId, string deletedBy, string baseUrl)
    {
        var storage = tenantFactory.Create(appId);
        var accountInformation = await storage.GetAccountInformation();
        if (accountInformation == null)
        {
            throw new ApiException("app_not_found", "App was not found.", 400);
        }

        if (accountInformation.DeleteAt.HasValue)
        {
            throw new ApiException("app_pending_deletion", "App is already pending to be deleted.", 400);
        }

        bool canDeleteImmediately = accountInformation.CreatedAt > _systemClock.UtcNow.AddDays(-3);

        if (!canDeleteImmediately)
        {
            canDeleteImmediately = !(await storage.HasUsersAsync());
        }

        if (canDeleteImmediately)
        {
            await storage.DeleteAccount();
            return new AppDeletionResult(
                $"The app '{accountInformation.AcountName}' was deleted.",
                true,
                _systemClock.UtcNow.UtcDateTime,
                accountInformation.AdminEmails);
        }

        // Lock/Freeze all API keys that have been issued.
        await storage.LockAllApiKeys(true);

        var deleteAt = _systemClock.UtcNow.AddMonths(1).UtcDateTime;
        await storage.SetAppDeletionDate(deleteAt);

        return new AppDeletionResult(
            $"The app '{accountInformation.AcountName}' will be deleted at '{deleteAt}'.",
            false,
            deleteAt,
            accountInformation.AdminEmails);
    }

    public async Task<IEnumerable<string>> GetApplicationsPendingDeletionAsync()
    {
        var storage = _globalStorageFactory.Create();
        var tenants = await storage.GetApplicationsPendingDeletionAsync();
        return tenants;
    }

    public async Task SetFeaturesAsync(string appId, ManageFeaturesRequest payload)
    {
        if (payload == null)
        {
            throw new ApiException("No 'body' or 'parameters' were passed.", 400);
        }

        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new ApiException($"'{nameof(appId)}' is required.", 400);
        }

        var storage = tenantFactory.Create(appId);
        await storage.SetFeaturesAsync(payload);
    }

    public async Task<AppFeatureResponse> GetFeaturesAsync(string appId)
    {
        var storage = tenantFactory.Create(appId);
        var entity = await storage.GetAppFeaturesAsync();
        var dto = entity.ToDto();
        return dto;
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreatePublicKeyRequest payload)
    {
        if (payload.Scopes == null || !payload.Scopes.Any())
        {
            throw new ApiException("create_api_key_scopes_required", "Please select at least one scope.", 400);
        }

        var storage = tenantFactory.Create(appId);
        var publicKeyResult = await SetupApiKey(appId, storage, payload.Scopes.ToArray());
        return new CreateApiKeyResponse(publicKeyResult);
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreateSecretKeyRequest payload)
    {
        if (payload.Scopes == null || !payload.Scopes.Any())
        {
            throw new ApiException("create_api_key_scopes_required", "Please select at least one scope.", 400);
        }

        var storage = tenantFactory.Create(appId);
        var secretKeyResult = await SetupApiSecret(appId, storage, payload.Scopes.ToArray());
        return new CreateApiKeyResponse(secretKeyResult.original);
    }

    public async Task<IReadOnlyCollection<ApiKeyResponse>> ListApiKeysAsync(string appId)
    {
        var storage = tenantFactory.Create(appId);
        var keys = await storage.GetAllApiKeys();
        var dtos = keys.Select(x => new ApiKeyResponse(
            x.Id,
            x.ApiKey.Contains("public") ? x.ApiKey : x.MaskedApiKey,
            x.ApiKey.Contains("public") ? ApiKeyTypes.Public : ApiKeyTypes.Secret,
            x.Scopes.Order().ToHashSet(),
            x.IsLocked,
            x.LastLockedAt
        )).ToImmutableList();
        return dtos;
    }

    public async Task LockApiKeyAsync(string appId, string apiKeyId)
    {
        var storage = tenantFactory.Create(appId);
        try
        {
            await storage.LockApiKeyAsync(apiKeyId);
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Apikey was not found. {AppId} {ApiKey}", appId, apiKeyId);
            throw new ApiException("api_key_not_found", "Apikey was not found", 404);
        }
    }

    public async Task UnlockApiKeyAsync(string appId, string apiKeyId)
    {
        var storage = tenantFactory.Create(appId);
        try
        {
            await storage.UnlockApiKeyAsync(apiKeyId);
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Apikey was not found. {AppId} {ApiKey}", appId, apiKeyId);
            throw new ApiException("api_key_not_found", "Apikey was not found", 404);
        }
    }

    public async Task DeleteApiKeyAsync(string appId, string apiKeyId)
    {
        var storage = tenantFactory.Create(appId);
        try
        {
            await storage.DeleteApiKeyAsync(apiKeyId);
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Apikey was not found. {AppId} {ApiKey}", appId, apiKeyId);
            throw new ApiException("api_key_not_found", "Apikey was not found", 404);
        }
    }

    public Task EnableGenerateSignInTokenEndpoint(string appId)
    {
        var storage = tenantFactory.Create(appId);

        return storage.EnableGenerateSignInTokenEndpoint();
    }

    public Task DisableGenerateSignInTokenEndpoint(string appId)
    {
        var storage = tenantFactory.Create(appId);

        return storage.DisableGenerateSignInTokenEndpoint();
    }

    private static Task<(string original, string hashed)> SetupApiSecret(string accountName, ITenantStorage storage)
    {
        return SetupApiSecret(accountName, storage, new[] { SecretKeyScopes.TokenRegister, SecretKeyScopes.TokenVerify });
    }

    private static async Task<(string original, string hashed)> SetupApiSecret(string accountName, ITenantStorage storage, SecretKeyScopes[] scopes)
    {
        var secretKey = ApiKeyUtils.GeneratePrivateApiKey(accountName, "secret");
        // last 4 chars
        var pk2 = secretKey.originalApiKey.Substring(secretKey.originalApiKey.Length - 4);
        await storage.StoreApiKey(pk2, secretKey.hashedApiKey, scopes.Select(x => x.GetValue()).ToArray());
        return secretKey;
    }

    private static Task<string> SetupApiKey(string accountName, ITenantStorage storage)
    {
        return SetupApiKey(accountName, storage, new[] { PublicKeyScopes.Register, PublicKeyScopes.Login });
    }

    private static async Task<string> SetupApiKey(string accountName, ITenantStorage storage, PublicKeyScopes[] scopes)
    {
        // create tenant and store apikey
        var publicKey = ApiKeyUtils.GeneratePublicApiKey(accountName, "public");
        // last 4 chars
        var pk = publicKey.Substring(publicKey.Length - 4);
        await storage.StoreApiKey(pk, publicKey, scopes.Select(x => x.GetValue()).ToArray());
        return publicKey;
    }
}