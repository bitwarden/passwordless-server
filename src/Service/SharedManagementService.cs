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
using SendGrid;
using SendGrid.Helpers.Mail;

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
    Task<AppDeletionResult> MarkDeleteApplicationAsync(string appId, string deletedBy);
    Task<AppDeletionResult> DeleteAccount(string appId, string cancelLink);
    Task SendAbortEmail(EmailAboutAccountDeletion input);
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

    public async Task<AppDeletionResult> MarkDeleteApplicationAsync(string appId, string deletedBy)
    {
        var storage = tenantFactory.Create(appId);
        var accountInformation = await storage.GetAccountInformation();
        if (accountInformation == null)
        {
            throw new ApiException("app_not_found", "App was not found.", 400);
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
        else
        {
            var deleteAt = _systemClock.UtcNow.AddDays(14).UtcDateTime;
            await storage.SetAppDeletionDate(deleteAt);
            await _mailService.SendApplicationToBeDeletedAsync(accountInformation, deletedBy);
            return new AppDeletionResult($"The app '{accountInformation.AcountName}' will be deleted at '{deleteAt}'.", false, deleteAt);
        }
    }


    public async Task<AppDeletionResult> DeleteAccount(string appId, string cancelLink)
    {
        var storage = tenantFactory.Create(appId);
        var app = await storage.GetAccountInformation();
        // check if we have credentials and the app was newly created
        // if no credentials, remove.
        var userCount = await storage.GetUsersCount();
        if (userCount == 0 && app.CreatedAt > DateTime.UtcNow.AddDays(-3))
        {
            await storage.DeleteAccount();
            return new AppDeletionResult("The App was deleted because it had no users.", true, DateTime.UtcNow);
        }

        // check if all api keys are locked more than 14 days ago
        var now = DateTime.UtcNow;
        List<ApiKeyDesc> keys = await storage.GetAllApiKeys();
        var haveBeenLocked = keys.All(x => x.IsLocked && x.LastLockedAt < now.AddDays(-14));
        if (!haveBeenLocked)
        {
            await FreezeAccount(appId);
            await storage.SetAppDeletionDate(now.AddDays(30));
            var sendConfirmationEmailInput = new EmailAboutAccountDeletion
            {
                CancelLink = cancelLink,
                Emails = app.AdminEmails,
                AccountName = app.AcountName,
                Message =
                    $"Your Passwordless.dev app '{app.AcountName}' has been frozen because the account/delete endpoint was called. Your data will be deleted after 30 days. To stop this, please visit the URL: " +
                    cancelLink
            };

            // Send warning email with url to abort
            await SendAbortEmail(sendConfirmationEmailInput);
            return new AppDeletionResult(
                $"All API keys have now been frozen. It will be deleted in 30 days. Please visit this link to cancel: {cancelLink}",
                false, null);
        }

        // Check MarkedForDeletionAt is set, otherwise set it to 14 days in the future

        if (app.DeleteAt == null)
        {
            var deletionAt = now.AddDays(14);

            var sendConfirmationEmailInput = new EmailAboutAccountDeletion
            {
                CancelLink = cancelLink,
                Emails = app.AdminEmails,
                AccountName = app.AcountName,
                Message =
                    $"Your Passwordless.dev app '{app.AcountName}' has now been frozen for 14 days and will be permanently deleted at: {deletionAt}. To stop this, please visit the URL: " +
                    cancelLink
            };

            // Send warning email with url to abort
            await SendAbortEmail(sendConfirmationEmailInput);
            return new AppDeletionResult("The App was marked for deletion. It will be deleted in 14 days.", false,
                deletionAt);
        }
        else if (app.DeleteAt < now)
        {
            await storage.DeleteAccount();
            var sendConfirmationEmailInput = new EmailAboutAccountDeletion
            {
                CancelLink = cancelLink,
                Emails = app.AdminEmails,
                AccountName = app.AcountName,
                Message = $"Your Passwordless.dev app '{app.AcountName}' has now been permanently deleted"
            };

            // Send warning email with url to abort
            await SendAbortEmail(sendConfirmationEmailInput);
            return new AppDeletionResult("The app was deleted because it had been marked for deletion.", true,
                app.DeleteAt);
        }

        return new AppDeletionResult("The app is marked for deletion. It will be deleted in 14 days.", false,
            app.DeleteAt);
    }

    public async Task SendAbortEmail(EmailAboutAccountDeletion input)
    {
        var x = input.Emails.ToList();

        var emails = x.Select(x => new EmailAddress(x)).ToList();

        var message = new SendGridMessage();
        message.AddTos(emails);
        message.AddBcc("account-deletion@passwordless.dev");

        message.SetSubject(input.AccountName + " Passwordless account deletion and data loss process");
        message.PlainTextContent = input.Message;

        message.SetFrom(new EmailAddress("noreply@passwordless.dev", "Passwordless Support"));
        message.SetReplyTo(new EmailAddress("support@passwordless.dev", "Passwordless Support"));
        message.SetClickTracking(false, false);

        var client = new SendGridClient(config["SENDGRID_API_KEY"]);

        try
        {
            var res = await client.SendEmailAsync(message);
            if (res.StatusCode != System.Net.HttpStatusCode.Accepted)
                throw new Exception("Sendgrid failure. Status:" + res.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send DeletionEmail to {Emails} for {Account} with CancelLink {CancelLink}",
                emails, input.AccountName, input.CancelLink);
            throw;
        }

        _logger.LogWarning("Email sent to {Emails} for {Account} with CancelLink {CancelLink}", emails,
            input.AccountName, input.CancelLink);
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