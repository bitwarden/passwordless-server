using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Utils;
using Passwordless.Service.Extensions.Models;
using Passwordless.Service.Models;
using AuthenticationConfiguration = Passwordless.Service.Models.AuthenticationConfiguration;

namespace Passwordless.Service.Storage.Ef;

public class EfTenantStorage(
    DbTenantContext db,
    TimeProvider timeProvider,
    ITenantProvider tenantProvider)
    : ITenantStorage
{
    public string Tenant => tenantProvider.Tenant;

    public Task<ApiKeyDesc> GetApiKeyAsync(string apiKey)
    {
        var appId = ApiKeyUtils.GetAppId(apiKey);
        var pk = apiKey.Substring(apiKey.Length - 4);
        return db.ApiKeys.FirstOrDefaultAsync(e => e.Id == pk && e.Tenant == appId);
    }

    public async Task AddCredentialToUser(Fido2User user, StoredCredential cred)
    {
        db.Credentials.Add(EFStoredCredential.FromStoredCredential(cred, tenantProvider.Tenant));
        await db.SaveChangesAsync();
    }

    public async Task AddTokenKey(TokenKey tokenKey)
    {
        db.TokenKeys.Add(tokenKey);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAccount()
    {
        await db.Aliases.ExecuteDeleteAsync();
        await db.ApiKeys.ExecuteDeleteAsync();
        await db.TokenKeys.ExecuteDeleteAsync();
        await db.Credentials.ExecuteDeleteAsync();
        await db.AccountInfo.ExecuteDeleteAsync();
        await db.AppFeatures.ExecuteDeleteAsync();
        await db.Authenticators.ExecuteDeleteAsync();
        await db.PeriodicCredentialReports.ExecuteDeleteAsync();
        await db.PeriodicActiveUserReports.ExecuteDeleteAsync();
    }

    public Task DeleteCredential(byte[] id)
    {
        return db.Credentials.Where(e => e.DescriptorId == id).ExecuteDeleteAsync();
    }

    public Task<bool> ExistsAsync(byte[] credentialId)
    {
        return db.Credentials.AnyAsync(e => e.DescriptorId == credentialId);
    }

    public Task<AccountMetaInformation> GetAccountInformation()
    {
        return db.AccountInfo.FirstOrDefaultAsync();
    }

    public Task<AppFeature> GetAppFeaturesAsync()
    {
        return db.AppFeatures.FirstOrDefaultAsync();
    }

    public async Task<List<AliasPointer>> GetAliasesByUserId(string userid)
    {
        var res = await db.Aliases.Where(a => a.UserId == userid).ToListAsync();
        return res;
    }

    public async Task<StoredCredential> GetCredential(byte[] credentialId)
    {
        var res = await db.Credentials.FirstOrDefaultAsync(c => c.DescriptorId == credentialId);

        return res?.ToStoredCredential();
    }

    public async Task<List<StoredCredential>> GetCredentialsByUserIdAsync(string userId)
    {
        var res = await db.Credentials.Where(c => c.UserId == userId).ToListAsync();

        return res.Select(c => c.ToStoredCredential()).ToList();
    }

    public async Task<List<StoredCredential>> GetCredentials(IEnumerable<byte[]> credentialIds)
    {
        var res = await db.Credentials.Where(c => credentialIds.Contains(c.DescriptorId)).ToListAsync();

        return res.Select(c => c.ToStoredCredential()).ToList();
    }

    public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias)
    {
        var aliases = await db.Aliases.FirstOrDefaultAsync(a => a.Alias == alias);
        if (aliases == null)
        {
            return new List<PublicKeyCredentialDescriptor>();
        }

        var userid = aliases.UserId;
        // Do we really need these AsNoTracking?
        var descs = await db.Credentials.Where(c => c.UserId == userid).ToListAsync();

        var pkcred = descs.Select(x => new PublicKeyCredentialDescriptor(x.DescriptorType.Value, x.DescriptorId, x.DescriptorTransports)).ToList();
        return pkcred;
    }

    public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true)
    {
        // TODO: Returning a c.descriptor does not work.
        var res = await db.Credentials.Where(c => c.UserId == userId).ToListAsync();

        var pkcred = res.Select(x => new PublicKeyCredentialDescriptor(x.DescriptorType.Value, x.DescriptorId, x.DescriptorTransports)).ToList();

        return pkcred;
    }

    public Task<List<TokenKey>> GetTokenKeys()
    {
        return db.TokenKeys.ToListAsync();
    }

    public async Task<string> GetUserIdByAliasAsync(string alias)
    {
        var res = await db.Aliases.Where(a => a.Alias == alias).Select(a => a.UserId).FirstOrDefaultAsync();
        return res;
    }

    public async Task<int> GetUsersCount()
    {
        var res = await db.Credentials.Select(c => c.UserId).Distinct().CountAsync();

        return res;
    }

    public async Task<bool> HasUsersAsync()
    {
        var res = await db.Credentials.AnyAsync();
        return res;
    }

    public async Task DeleteUser(string userId)
    {
        await db.Credentials.Where(c => c.UserId == userId).ExecuteDeleteAsync();
        await db.Aliases.Where(c => c.UserId == userId).ExecuteDeleteAsync();
    }

    public Task<List<ApiKeyDesc>> GetAllApiKeys()
    {
        return db.ApiKeys.ToListAsync();
    }

    public async Task SetAppDeletionDate(DateTime? deletionAt)
    {
        await db.AccountInfo.ExecuteUpdateAsync(x => x.SetProperty(a => a.DeleteAt, deletionAt));
    }

    public async Task<bool> CheckIfAliasIsAvailable(IEnumerable<string> aliases, string userId)
    {
        return !await db.Aliases.AnyAsync(a => aliases.Contains(a.Alias) && a.UserId != userId);
    }

    public async Task SetFeaturesAsync(SetFeaturesRequest features) =>
        await db.AppFeatures.ExecuteUpdateAsync(x => x
            .SetProperty(f => f.IsGenerateSignInTokenEndpointEnabled,
                existing => features.EnableManuallyGeneratedAuthenticationTokens ??
                            existing.IsGenerateSignInTokenEndpointEnabled
            )
            .SetProperty(f => f.IsMagicLinksEnabled,
                existing => features.EnableMagicLinks ?? existing.IsMagicLinksEnabled
            )
            .SetProperty(f => f.EventLoggingRetentionPeriod,
                existing => features.EventLoggingRetentionPeriod.HasValue
                    ? features.EventLoggingRetentionPeriod.Value
                    : existing.EventLoggingRetentionPeriod
            )
        );

    public async Task SetFeaturesAsync(ManageFeaturesRequest features)
    {
        var existingEntity = await db.AppFeatures.FirstOrDefaultAsync();
        existingEntity.EventLoggingIsEnabled = features.EventLoggingIsEnabled;
        existingEntity.EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod;
        existingEntity.MagicLinkEmailMonthlyQuota = features.MagicLinkEmailMonthlyQuota;
        existingEntity.MaxUsers = features.MaxUsers;
        existingEntity.AllowAttestation = features.AllowAttestation;
        await db.SaveChangesAsync();
    }

    public async Task LockApiKeyAsync(string apiKeyId)
    {
        var rows = await db.ApiKeys
            .Where(x => x.Id == apiKeyId)
            .ExecuteUpdateAsync(apiKey => apiKey
                .SetProperty(x => x.IsLocked, true)
                .SetProperty(x => x.LastLockedAt, timeProvider.GetUtcNow().UtcDateTime)
        );
        if (rows == 0)
        {
            throw new ArgumentException("ApiKey not found");
        }
    }

    public async Task UnlockApiKeyAsync(string apiKeyId)
    {
        var rows = await db.ApiKeys
            .Where(x => x.Id == apiKeyId)
            .ExecuteUpdateAsync(apiKey => apiKey
                .SetProperty(x => x.IsLocked, false)
                .SetProperty(x => x.LastUnlockedAt, timeProvider.GetUtcNow().UtcDateTime)
            );
        if (rows == 0)
        {
            throw new ArgumentException("ApiKey not found");
        }
    }

    public async Task DeleteApiKeyAsync(string apiKeyId)
    {
        var rows = await db.ApiKeys
            .Where(x => x.Id == apiKeyId)
            .ExecuteDeleteAsync();
        if (rows == 0)
        {
            throw new ArgumentException("ApiKey not found");
        }
    }

    public async Task<IEnumerable<PeriodicCredentialReport>> GetPeriodicCredentialReportsAsync(
        DateOnly? from,
        DateOnly? to)
    {
        var query = db.PeriodicCredentialReports.AsQueryable();
        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }
        return await query
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PeriodicActiveUserReport>> GetPeriodicActiveUserReportsAsync(DateOnly? from, DateOnly? to)
    {
        var query = db.PeriodicActiveUserReports.AsQueryable();
        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }
        return await query
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<Authenticator>> GetAuthenticatorsAsync(bool? isAllowed = null)
    {
        IQueryable<Authenticator> query = db.Authenticators;

        if (isAllowed.HasValue)
        {
            query = query.Where(x => x.IsAllowed == isAllowed.Value);
        }

        return await query.ToListAsync();
    }

    public async Task AddAuthenticatorsAsync(IEnumerable<Guid> aaGuids, bool isAllowed)
    {
        var existingAuthenticators = db.Authenticators.Where(x => aaGuids.Contains(x.AaGuid));

        await existingAuthenticators.ExecuteUpdateAsync(x => x
                .SetProperty(entity => entity.IsAllowed, isAllowed)
            );

        var existingAaGuids = await existingAuthenticators.Select(x => x.AaGuid).ToListAsync();
        var newAuthenticators = aaGuids
            .Where(x => !existingAaGuids.Contains(x))
            .Select(x => new Authenticator
            {
                AaGuid = x,
                IsAllowed = isAllowed,
                CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
                Tenant = tenantProvider.Tenant
            }).ToList();

        db.Authenticators.AddRange(newAuthenticators);
        await db.SaveChangesAsync();
    }

    public Task RemoveAuthenticatorsAsync(IEnumerable<Guid> aaGuids)
    {
        return db.Authenticators
            .Where(x => aaGuids.Contains(x.AaGuid))
            .ExecuteDeleteAsync();
    }

    public async Task<IReadOnlyList<DispatchedEmail>> GetDispatchedEmailsAsync(TimeSpan window)
    {
        var from = timeProvider.GetUtcNow().UtcDateTime - window;
        return await db.DispatchedEmails
            .Where(x => x.CreatedAt >= from)
            .ToArrayAsync();
    }

    public async Task<int> GetDispatchedEmailCountAsync(TimeSpan window)
    {
        var from = timeProvider.GetUtcNow().UtcDateTime - window;
        return await db.DispatchedEmails
            .CountAsync(x => x.CreatedAt >= from);
    }

    public async Task<DispatchedEmail> AddDispatchedEmailAsync(string userId, string emailAddress, string linkTemplate)
    {
        var email = new DispatchedEmail
        {
            Tenant = Tenant,
            Id = Guid.NewGuid(),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            UserId = userId,
            EmailAddress = emailAddress,
            LinkTemplate = linkTemplate
        };

        db.DispatchedEmails.Add(email);
        await db.SaveChangesAsync();

        return email;
    }

    public async Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync(
        GetAuthenticationConfigurationsFilter filter)
    {
        var query = db.AuthenticationConfigurations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Purpose))
        {
            query = query.Where(x => x.Purpose == filter.Purpose);
        }

        var configurations = await query.ToListAsync();

        return configurations.Select(x => x.ToDto());
    }

    public async Task CreateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration)
    {
        db.AuthenticationConfigurations.Add(new AuthenticationConfiguration
        {
            Purpose = configuration.Purpose.Value,
            UserVerificationRequirement = configuration.UserVerificationRequirement,
            TimeToLive = configuration.TimeToLive,
            Hints = configuration.Hints,
            Tenant = Tenant,
            CreatedBy = configuration.CreatedBy ?? string.Empty,
            CreatedOn = configuration.CreatedOn?.UtcDateTime
        });

        await db.SaveChangesAsync();
    }

    public async Task UpdateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration)
    {
        await db.AuthenticationConfigurations
            .Where(x => x.Purpose == configuration.Purpose.Value)
            .ExecuteUpdateAsync(x => x
                .SetProperty(c => c.UserVerificationRequirement, configuration.UserVerificationRequirement)
                .SetProperty(c => c.TimeToLive, configuration.TimeToLive)
                .SetProperty(c => c.Hints, configuration.Hints)
                .SetProperty(c => c.EditedBy, configuration.EditedBy)
                .SetProperty(c => c.EditedOn, configuration.EditedOn.HasValue ? configuration.EditedOn.Value.UtcDateTime : null)
                .SetProperty(c => c.LastUsedOn, configuration.LastUsedOn.HasValue ? configuration.LastUsedOn.Value.UtcDateTime : null));
    }

    public async Task DeleteAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration)
    {
        await db.AuthenticationConfigurations.Where(x => x.Purpose == configuration.Purpose.Value)
            .ExecuteDeleteAsync();
    }

    public async Task LockAllApiKeys(bool isLocked)
    {
        await db.ApiKeys.ExecuteUpdateAsync(x => x
            .SetProperty(k => k.IsLocked, isLocked)
            .SetProperty(k => isLocked ? k.LastLockedAt : k.LastUnlockedAt, DateTime.UtcNow)
        );
    }

    public Task RemoveTokenKey(int keyId)
    {
        return db.TokenKeys.Where(k => k.KeyId == keyId).ExecuteDeleteAsync();
    }

    public Task RemoveExpiredTokenKeys(CancellationToken cancellationToken)
    {
        return db.TokenKeys.Where(x => x.CreatedAt < timeProvider.GetUtcNow().AddDays(-30).DateTime)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task SaveAccountInformation(AccountMetaInformation info)
    {
        db.AccountInfo.Add(info);
        await db.SaveChangesAsync();
    }

    public async Task StoreAlias(string userid, Dictionary<string, string> aliases)
    {
        var pointers = aliases.Select(a => new AliasPointer() { Tenant = tenantProvider.Tenant, UserId = userid, Alias = a.Key, Plaintext = a.Value });
        db.Aliases.RemoveRange(db.Aliases.Where(ap => ap.UserId == userid));
        db.Aliases.AddRange(pointers);
        await db.SaveChangesAsync();
    }

    public async Task StoreApiKey(string pkpart, string apikey, string[] scopes)
    {
        var ak = new ApiKeyDesc
        {
            Tenant = tenantProvider.Tenant,
            Id = pkpart,
            ApiKey = apikey,
            Scopes = scopes,
            IsLocked = false
        };
        db.ApiKeys.Add(ak);
        await db.SaveChangesAsync();
    }

    public async Task<bool> TenantExists()
    {
        return (await db.AccountInfo.FirstOrDefaultAsync()) != null;
    }

    public async Task UpdateCredential(byte[] credentialId, uint counter, string country, string device)
    {
        var c = await db.Credentials.Where(c => c.DescriptorId == credentialId).FirstOrDefaultAsync();
        c.SignatureCounter = counter;
        c.Country = country;
        c.Device = device;
        c.LastUsedAt = DateTime.UtcNow;
        db.Credentials.Update(c);
        await db.SaveChangesAsync();
    }

    public async Task<List<UserSummary>> GetUsers(string lastUserId)
    {
        var credentialsPerUser = await db.Credentials
            .OrderBy(c => c.CreatedAt)
            .GroupBy(c => c.UserId)
            .Select((g) =>
                new { UserId = g.Key, LastUsedAt = g.Max(c => c.LastUsedAt), Count = g.Count() })
            .Take(1000)
            .ToListAsync();

        var aliasesPerUser = await db.Aliases
            .GroupBy(a => a.UserId)
            .Select((g) =>
                new { UserId = g.Key, Count = g.Count(), Aliases = g.Select(a => a.Plaintext) })
            .Take(1000)
            .ToListAsync();

        var userSummaries = new Dictionary<string, UserSummary>();
        foreach (var cred in credentialsPerUser)
        {
            if (!userSummaries.TryGetValue(cred.UserId, out var summary))
            {
                summary = new UserSummary();
                userSummaries.Add(cred.UserId, summary);
            }

            summary.UserId = cred.UserId;
            summary.CredentialsCount = cred.Count;
            summary.LastUsedAt = cred.LastUsedAt;
        }

        foreach (var alias in aliasesPerUser)
        {
            if (!userSummaries.TryGetValue(alias.UserId, out var summary))
            {
                summary = new UserSummary();
                userSummaries.Add(alias.UserId, summary);
            }

            summary.UserId = alias.UserId;
            summary.AliasCount = alias.Count;
            summary.Aliases = alias.Aliases.ToList();
        }

        return userSummaries.Values.ToList();
    }
}