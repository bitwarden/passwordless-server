using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Passwordless.Service.Helpers;
using Passwordless.Service.Mail;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public record AppDeletionResult(string Message, bool IsDeleted, DateTime? DeleteAt);

public interface ISharedManagementService
{
    Task<bool> IsAvailable(string appId);
    Task<AccountKeysCreation> GenerateAccount(string accountName, string adminEmail);
    Task<string> ValidateSecretKey(string secretKey);
    Task<string> ValidatePublicKey(string publicKey);
    Task FreezeAccount(string accountName);
    Task UnFreezeAccount(string accountName);
    Task<AppDeletionResult> DeleteApplicationAsync(string appId);
    Task<AppDeletionResult> MarkDeleteApplicationAsync(string appId, string deletedBy);
}

public class SharedManagementService : ISharedManagementService
{
    private readonly ILogger _logger;
    private readonly IConfiguration config;
    private readonly ISystemClock _systemClock;
    private readonly ITenantStorageFactory tenantFactory;
    private readonly IMailService _mailService;

    public SharedManagementService(ITenantStorageFactory tenantFactory,
        IMailService mailService,
        IConfiguration config,
        ISystemClock systemClock,
        ILogger<SharedManagementService> logger)
    {
        this.tenantFactory = tenantFactory;
        _mailService = mailService;
        this.config = config;
        _systemClock = systemClock;
        _logger = logger;
    }


    public async Task<bool> IsAvailable(string accountName)
    {
        // check if tenant already exists
        var storage = tenantFactory.Create(accountName);
        return !await storage.TenantExists();
    }

    public async Task<AccountKeysCreation> GenerateAccount(string accountName, string adminEmail)
    {
        if (string.IsNullOrWhiteSpace(accountName) || string.IsNullOrWhiteSpace(adminEmail))
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
            SubscriptionTier = "Free"
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
            existingKey.CheckLocked();

            if (CheckApiKeyMatch(existingKey.ApiKey, secretKey))
            {
                return appId;
            }
        }

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
            existingKey.CheckLocked();
            return appId;
        }

        _logger.LogWarning("Apikey was not valid. {AppId} {ApiKey}", appId, publicKey);
        throw new ApiException("Apikey was not valid", 401);
    }

    private string GetAppId(string apiKey)
    {
        try
        {
            return ParseAppId(apiKey);
        }
        catch (Exception)
        {
            _logger.LogError("Could not parse accountname={apikey}", apiKey);
            throw new ApiException("Please supply the apikey or apisecret header with correct value.", 401);
        }
    }

    private static string ParseAppId(string apiKey)
    {
        ReadOnlySpan<char> span = apiKey.AsSpan();
        var i = span.IndexOf(':');
        return span[..i].ToString();
    }

    private static bool CheckApiKeyMatch(string hash, string input)
    {
        try
        {
            var parts = hash.Split(':');

            var salt = Convert.FromBase64String(parts[0]);

            var bytes = KeyDerivation.Pbkdf2(input, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);

            return parts[1].Equals(Convert.ToBase64String(bytes));
        }
        catch
        {
            return false;
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
        // lock API keys?
        // send email to admin
        // queue deletion
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
        await _mailService.SendApplicationDeletedAsync(accountInformation, "system");
        return new AppDeletionResult($"The app '{accountInformation.AcountName}' was deleted.", true,
            _systemClock.UtcNow.UtcDateTime);
    }

    public async Task<AppDeletionResult> MarkDeleteApplicationAsync(string appId, string deletedBy)
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
            await _mailService.SendApplicationDeletedAsync(accountInformation, deletedBy);
            return new AppDeletionResult($"The app '{accountInformation.AcountName}' was deleted.", true,
                _systemClock.UtcNow.UtcDateTime);
        }

        // Lock/Freeze all API keys that have been issued.
        await storage.LockAllApiKeys(true);

        var deleteAt = _systemClock.UtcNow.AddMonths(1).UtcDateTime;
        await storage.SetAppDeletionDate(deleteAt);
        await _mailService.SendApplicationToBeDeletedAsync(accountInformation, deletedBy);
        return new AppDeletionResult($"The app '{accountInformation.AcountName}' will be deleted at '{deleteAt}'.", false, deleteAt);
    }

    private static async Task<(string original, string hashed)> SetupApiSecret(string accountName,
        ITenantStorage storage)
    {
        var secretKey = GenerateSecretKey(accountName, "secret");
        // last 4 chars
        var pk2 = secretKey.original.Substring(secretKey.original.Length - 4);
        await storage.StoreApiKey(pk2, secretKey.hashed, new string[] { "token_register", "token_verify" });
        return secretKey;
    }

    private static async Task<string> SetupApiKey(string accountName, ITenantStorage storage)
    {
        // create tenant and store apikey
        var publicKey = GeneratePublicKey(accountName, "public");
        // last 4 chars
        var pk = publicKey.Substring(publicKey.Length - 4);
        await storage.StoreApiKey(pk, publicKey, new string[] { "register", "login" });
        return publicKey;
    }

    private static string GeneratePublicKey(string accountName, string prefix)
    {
        var key = GuidHelper.CreateCryptographicallySecureRandomRFC4122Guid().ToString("N");

        return accountName + ":" + prefix + ":" + key;
    }

    private static (string original, string hashed) GenerateSecretKey(string accountName, string prefix)
    {
        var key = GuidHelper.CreateCryptographicallySecureRandomRFC4122Guid().ToString("N");


        var original = accountName + ":" + prefix + ":" + key;

        var hashed = CalculateHash(original);

        return (original, hashed);
    }

    private static string CalculateHash(string input)
    {
        var salt = GenerateSalt(16);

        var bytes = KeyDerivation.Pbkdf2(input, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(bytes)}";
    }

    private static byte[] GenerateSalt(int length)
    {
        var salt = new byte[length];

        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(salt);
        }

        return salt;
    }
}