using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Service.Models;
using Service.Storage;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Linq;
using Service.Helpers;

namespace Service
{
    public class AccountService
    {
        private ILogger log;
        private readonly IConfiguration config;
        private readonly IStorage storage;

        private readonly IDbTenantContextFactory tenantFactory;

        public AccountService(ILogger log, IConfiguration config, IStorage storage, IDbTenantContextFactory tenantFactory)
        {
            this.log = log;
            this.config = config;
            this.storage = storage;
            this.tenantFactory = tenantFactory;
        }

        public async Task<string> ValidatePublicApiKey(string apiKey)
        {
            var tenant = GetAccountName(apiKey);


            var res = await storage.GetApiKeyAsync(apiKey);
            if (res != null && res.ApiKey == apiKey)
            {
                res.CheckLocked();
                return tenant;
            }

            log.LogInformation("Apikey was not valid. tenant={tenant} apikey={apikey}", tenant, apiKey);
            throw new ApiException("Apikey was not valid", 401);
        }

        public async Task<string> ValidateSecretApiKey(string apiKey)
        {
            var tenant = GetAccountName(apiKey);


            var res = await storage.GetApiKeyAsync(apiKey);
            if (res != null)
            {
                res.CheckLocked();

                if (CheckApiKeyMatch(res.ApiKey, apiKey))
                {
                    return tenant;
                }
            }
            log.LogInformation("ApiSecret was not valid. tenant={tenant} apikey={apikey}", tenant, apiKey?.Substring(0, 20));
            throw new ApiException("ApiSecret was not valid", 401);
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

            IStorage newstorage = null;
            try
            {
                // This also checks if tenant already exists
                newstorage = await tenantFactory.CreateNewTenant(accountName);
            }
            catch (ArgumentException e)
            {
                throw new ApiException($"accountName '{accountName}' is not available", 409);
            }

            string ApiKey1 = await SetupApiKey(accountName, newstorage);
            string ApiKey2 = await SetupApiKey(accountName, newstorage);

            (string original, string hashed) apiSecret1 = await SetupApiSecret(accountName, newstorage);
            (string original, string hashed) apiSecret2 = await SetupApiSecret(accountName, newstorage);

            var account = new AccountMetaInformation()
            {
                AcountName = accountName,
                AdminEmails = new string[] { adminEmail },
                CreatedAt = DateTime.UtcNow,
                SubscriptionTier = "Free"
            };
            await newstorage.SaveAccountInformation(account);

            return new AccountKeysCreation
            {
                ApiKey1 = ApiKey1,
                ApiKey2 = ApiKey2,
                ApiSecret1 = apiSecret1.original,
                ApiSecret2 = apiSecret2.original,
                Message = "Store keys safely. They will only be shown to you once."
            };
        }

        private static async Task<(string original, string hashed)> SetupApiSecret(string accountName, IStorage storage)
        {
            var secretKey = GenerateSecretKey(accountName, "secret");
            // last 4 chars
            var pk2 = secretKey.original.Substring(secretKey.original.Length - 4);
            await storage.StoreApiKey(pk2, secretKey.hashed, new string[] { "token_register", "token_verify" });
            return secretKey;
        }

        private static async Task<string> SetupApiKey(string accountName, IStorage storage)
        {
            // create tenant and store apikey
            var publicKey = GeneratePublicKey(accountName, "public");
            // last 4 chars
            var pk = publicKey.Substring(publicKey.Length - 4);
            await storage.StoreApiKey(pk, publicKey, new string[] { "register", "login" });
            return publicKey;
        }

        public async Task<AccountMetaInformation> GetAccountInformation(string accountName)
        {
            return await storage.GetAccountInformation();
        }

        public async Task FreezeAccount(string accountName)
        {
            // lock API keys?
            // send email to admin
            // queue deletion
            await storage.LockAllApiKeys(true);
        }

        public async Task UnFreezeAccount(string accountName)
        {
            // lock API keys?
            // send email to admin
            // queue deletion
            await storage.LockAllApiKeys(false);
        }

        public async Task SendAbortEmail(ILogger log, IConfiguration config, EmailAboutAccountDeletion input)
        {
            var x = input.Emails.ToList();

            var emails = x.Select(x => new EmailAddress(x)).ToList();

            var message = new SendGridMessage();
            message.AddTos(emails);
            message.AddBcc("account-deletion@passwordless.dev");

            message.SetSubject(input.AccountName + " Passwordless account deletion and data loss process");
            message.PlainTextContent = input.Message;

            message.SetFrom(new EmailAddress("noreply@email.passwordless.dev", "Passwordless Account Service"));
            message.SetReplyTo(new EmailAddress("support@passwordless.dev", "Passwordless Account Support"));
            message.SetClickTracking(false, false);

            var client = new SendGridClient(config["SENDGRID_API_KEY"]);

            try
            {
                var res = await client.SendEmailAsync(message);
                if (res.StatusCode != System.Net.HttpStatusCode.Accepted) throw new Exception("Sendgrid failure. Status:" + res.StatusCode);

            }
            catch (Exception)
            {
                log.LogError($"Failed to send email to {String.Join(',', input.Emails)} for account {input.AccountName} with confirmation URL {input.CancelLink}");
                throw;
            }
            log.LogWarning($"[Deletion] Email sent to {String.Join(',', input.Emails)} for account {input.AccountName} with confirmation URL {input.CancelLink}");
        }

        public async Task DeleteAccount(string accountName)
        {
            await storage.DeleteAccount();
        }

        public bool CheckApiKeyMatch(string hash, string input)
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

        private static string GeneratePublicKey(string accountName, string prefix)
        {
            var key = GuidHelper.CreateCryptographicallySecureRandomRFC4122Guid().ToString("N");

            return accountName + ":" + prefix + ":" + key;
        }

        public static (string original, string hashed) GenerateSecretKey(string accountName, string prefix)
        {
            var key = GuidHelper.CreateCryptographicallySecureRandomRFC4122Guid().ToString("N");


            var original = accountName + ":" + prefix + ":" + key;

            var hashed = CalculateHash(original);

            return (original, hashed);
        }

        public static string CalculateHash(string input)
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

        public string GetAccountName(string apiKey)
        {
            try
            {
                return ParseAccountName(apiKey);
            }
            catch (Exception)
            {
                log.LogError("Could not parse accountname={apikey}", apiKey);
                throw new ApiException("Please supply the apikey or apisecret header with correct value.", 401);
            }
        }

        public string ParseAccountName(string apiKey)
        {
            var span = apiKey.AsSpan();
            var i = span.IndexOf(':');
            return span.Slice(0, i).ToString();
        }

        public async Task<Boolean> IsAvailable(string accountName)
        {
            // check if tenant already exists
            return !await storage.TenantExists();
        }
    }
}


