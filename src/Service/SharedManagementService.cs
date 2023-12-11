using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Constants;
using Passwordless.Common.Models;
using Passwordless.Common.Utils;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public record AppDeletionResult(string Message, bool IsDeleted, DateTime? DeleteAt, IReadOnlyCollection<string> AdminEmails);

public interface ISharedManagementService
{
    Task<bool> IsAvailable(string appId);
    Task<AccountKeysCreation> GenerateAccount(string appId, AppCreateDTO appCreationOptions);
    Task<string> ValidateSecretKey(string secretKey);
    Task<string> ValidatePublicKey(string publicKey);
    Task FreezeAccount(string accountName);
    Task UnFreezeAccount(string accountName);
    Task<AppDeletionResult> DeleteApplicationAsync(string appId);
    Task<AppDeletionResult> MarkDeleteApplicationAsync(string appId, string deletedBy, string baseUrl);
    Task<IEnumerable<string>> GetApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(string appId, ManageFeaturesDto payload);
    Task<AppFeatureDto> GetFeaturesAsync(string appId);
    Task<CreateApiKeyResultDto> CreateApiKeyAsync(string appId, CreateApiKeyDto payload);
    Task<IReadOnlyCollection<ApiKeyDto>> ListApiKeysAsync(string appId);
    Task LockApiKeyAsync(string appId, string apiKeyId);
    Task UnlockApiKeyAsync(string appId, string apiKeyId);
    Task DeleteApiKeyAsync(string appId, string apiKeyId);
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

    public async Task<AccountKeysCreation> GenerateAccount(string appId, AppCreateDTO appCreationOptions)
    {
        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new ApiException($"'{nameof(appId)}' cannot be null, empty or whitespace.", 400);
        }
        if (appCreationOptions == null)
        {
            throw new ApiException("No application creation options have been defined.", 400);
        }

        var accountName = appId;
        var adminEmail = appCreationOptions.AdminEmail;

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

        var account = new AccountMetaInformation()
        {
            AcountName = accountName,
            AdminEmails = new[] { adminEmail },
            CreatedAt = DateTime.UtcNow,
            SubscriptionTier = "Free",
            Features = new AppFeature
            {
                Tenant = accountName,
                EventLoggingIsEnabled = appCreationOptions.EventLoggingIsEnabled,
                EventLoggingRetentionPeriod = appCreationOptions.EventLoggingRetentionPeriod,
                MaxUsers = appCreationOptions.MaxUsers
            }
        };
        await storage.SaveAccountInformation(account);
        return new AccountKeysCreation
        {
            ApiKey1 = apiKey1,
            ApiKey2 = apiKey2,
            ApiSecret1 = apiSecret1.original,
            ApiSecret2 = apiSecret2.original,
            Message = "Store keys safely. They will only be shown to you once."
        };
    }

    public async Task<string> ValidateSecretKey(string secretKey)
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
                return appId;
            }
        }

        _eventLogger.LogInvalidApiSecretUsedEvent(_systemClock.UtcNow.UtcDateTime, appId, new ApplicationSecretKey(secretKey));
        _logger.LogInformation("ApiSecret was not valid. {AppId} {ApiKey}", appId, secretKey?[..20]);
        throw new ApiException("ApiSecret was not valid", 401);
    }

    public async Task<string> ValidatePublicKey(string publicKey)
    {
        var appId = GetAppId(publicKey);
        var storage = tenantFactory.Create(appId);

        var existingKey = await storage.GetApiKeyAsync(publicKey);
        if (existingKey != null && existingKey.ApiKey == publicKey)
        {
            if (!existingKey.IsLocked)
            {
                return appId;
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

    public async Task SetFeaturesAsync(string appId, ManageFeaturesDto payload)
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

    public async Task<AppFeatureDto> GetFeaturesAsync(string appId)
    {
        var storage = tenantFactory.Create(appId);
        var entity = await storage.GetAppFeaturesAsync();
        var dto = AppFeatureDto.FromEntity(entity);
        return dto;
    }

    public async Task<CreateApiKeyResultDto> CreateApiKeyAsync(string appId, CreateApiKeyDto payload)
    {
        if (payload.Scopes == null || !payload.Scopes.Any())
        {
            throw new ApiException("create_api_key_scopes_required", "Please select at least one scope.", 400);
        }

        switch (payload.Type)
        {
            case ApiKeyTypes.Public:
                if (payload.Scopes.All(x => ApiKeyScopes.PublicScopes.Contains(x)))
                {
                    throw new ApiException("create_api_key_scopes_invalid", "The request contains invalid scopes.", 400);
                }
                break;
            case ApiKeyTypes.Secret:
                if (payload.Scopes.All(x => ApiKeyScopes.SecretScopes.Contains(x)))
                {
                    throw new ApiException("create_api_key_scopes_invalid", "The request contains invalid scopes.", 400);
                }
                break;
        }

        var storage = tenantFactory.Create(appId);

        // We will clean this up later
        if (payload.Type == ApiKeyTypes.Public)
        {
            var publicKeyResult = await SetupApiKey(appId, storage, payload.Scopes.ToArray());
            return new CreateApiKeyResultDto(publicKeyResult);
        }
        else
        {
            var secretKeyResult = await SetupApiSecret(appId, storage, payload.Scopes.ToArray());
            return new CreateApiKeyResultDto(secretKeyResult.original);
        }
    }

    public async Task<IReadOnlyCollection<ApiKeyDto>> ListApiKeysAsync(string appId)
    {
        var storage = tenantFactory.Create(appId);
        var keys = await storage.GetAllApiKeys();
        var dtos = keys.Select(x => new ApiKeyDto
        {
            Id = x.Id,
            ApiKey = x.ApiKey.Contains("public") ? x.ApiKey : x.Id,
            Type = x.ApiKey.Contains("public") ? ApiKeyTypes.Public : ApiKeyTypes.Secret,
            Scopes = x.Scopes.ToHashSet(),
            IsLocked = x.IsLocked,
            LastLockedAt = x.LastLockedAt,
            LastUnlockedAt = x.LastUnlockedAt
        }).ToImmutableList();
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

    private static Task<(string original, string hashed)> SetupApiSecret(string accountName, ITenantStorage storage)
    {
        return SetupApiSecret(accountName, storage, new[] { "token_register", "token_verify" });
    }

    private static async Task<(string original, string hashed)> SetupApiSecret(string accountName, ITenantStorage storage, string[] scopes)
    {
        var secretKey = ApiKeyUtils.GeneratePrivateApiKey(accountName, "secret");
        // last 4 chars
        var pk2 = secretKey.originalApiKey.Substring(secretKey.originalApiKey.Length - 4);
        await storage.StoreApiKey(pk2, secretKey.hashedApiKey, new string[] { "token_register", "token_verify" });
        return secretKey;
    }

    private static Task<string> SetupApiKey(string accountName, ITenantStorage storage)
    {
        return SetupApiKey(accountName, storage, new[] { "register", "login" });
    }

    private static async Task<string> SetupApiKey(string accountName, ITenantStorage storage, string[] scopes)
    {
        // create tenant and store apikey
        var publicKey = ApiKeyUtils.GeneratePublicApiKey(accountName, "public");
        // last 4 chars
        var pk = publicKey.Substring(publicKey.Length - 4);
        await storage.StoreApiKey(pk, publicKey, scopes);
        return publicKey;
    }
}