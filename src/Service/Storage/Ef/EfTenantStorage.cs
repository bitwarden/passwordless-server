using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Passwordless.Common.Models.Apps;
using Passwordless.Common.Utils;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class EfTenantStorage : ITenantStorage
{
    private readonly DbTenantContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ITenantProvider _tenantProvider;

    public EfTenantStorage(
        DbTenantContext db,
        TimeProvider timeProvider,
        ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
        _timeProvider = timeProvider;
    }

    public string Tenant => _tenantProvider.Tenant;


    public Task<ApiKeyDesc> GetApiKeyAsync(string apiKey)
    {
        var appId = ApiKeyUtils.GetAppId(apiKey);
        var pk = apiKey.Substring(apiKey.Length - 4);
        return _db.ApiKeys.FirstOrDefaultAsync(e => e.Id == pk && e.Tenant == appId);
    }

    public async Task AddCredentialToUser(Fido2User user, StoredCredential cred)
    {
        _db.Credentials.Add(EFStoredCredential.FromStoredCredential(cred, _tenantProvider.Tenant));
        await _db.SaveChangesAsync();
    }

    public async Task AddTokenKey(TokenKey tokenKey)
    {
        _db.TokenKeys.Add(tokenKey);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAccount()
    {
        await _db.Aliases.ExecuteDeleteAsync();
        await _db.ApiKeys.ExecuteDeleteAsync();
        await _db.TokenKeys.ExecuteDeleteAsync();
        await _db.Credentials.ExecuteDeleteAsync();
        await _db.AccountInfo.ExecuteDeleteAsync();
        await _db.AppFeatures.ExecuteDeleteAsync();
        await _db.Authenticators.ExecuteDeleteAsync();
        await _db.PeriodicCredentialReports.ExecuteDeleteAsync();
        await _db.PeriodicActiveUserReports.ExecuteDeleteAsync();
    }

    public Task DeleteCredential(byte[] id)
    {
        return _db.Credentials.Where(e => e.DescriptorId == id).ExecuteDeleteAsync();
    }

    public Task<bool> ExistsAsync(byte[] credentialId)
    {
        return _db.Credentials.AnyAsync(e => e.DescriptorId == credentialId);
    }

    public Task<AccountMetaInformation> GetAccountInformation()
    {
        return _db.AccountInfo.FirstOrDefaultAsync();
    }

    public Task<AppFeature> GetAppFeaturesAsync()
    {
        return _db.AppFeatures.FirstOrDefaultAsync();
    }

    public async Task<List<AliasPointer>> GetAliasesByUserId(string userid)
    {
        var res = await _db.Aliases.Where(a => a.UserId == userid).ToListAsync();
        return res;
    }

    public async Task<StoredCredential> GetCredential(byte[] credentialId)
    {
        var res = await _db.Credentials.FirstOrDefaultAsync(c => c.DescriptorId == credentialId);

        return res?.ToStoredCredential();
    }

    public async Task<List<StoredCredential>> GetCredentialsByUserIdAsync(string userId)
    {
        var res = await _db.Credentials.Where(c => c.UserId == userId).ToListAsync();

        return res.Select(c => c.ToStoredCredential()).ToList();
    }

    public async Task<List<StoredCredential>> GetCredentials(IEnumerable<byte[]> credentialIds)
    {
        var res = await _db.Credentials.Where(c => credentialIds.Contains(c.DescriptorId)).ToListAsync();

        return res.Select(c => c.ToStoredCredential()).ToList();
    }

    public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias)
    {
        var aliases = await _db.Aliases.FirstOrDefaultAsync(a => a.Alias == alias);
        if (aliases == null)
        {
            return new List<PublicKeyCredentialDescriptor>();
        }

        var userid = aliases.UserId;
        // Do we really need these AsNoTracking?
        var descs = await _db.Credentials.Where(c => c.UserId == userid).ToListAsync();

        var pkcred = descs.Select(x => new PublicKeyCredentialDescriptor(x.DescriptorType.Value, x.DescriptorId, x.DescriptorTransports)).ToList();
        return pkcred;
    }

    public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true)
    {
        // TODO: Returning a c.descriptor does not work.
        var res = await _db.Credentials.Where(c => c.UserId == userId).ToListAsync();

        var pkcred = res.Select(x => new PublicKeyCredentialDescriptor(x.DescriptorType.Value, x.DescriptorId, x.DescriptorTransports)).ToList();

        return pkcred;
    }

    public Task<List<TokenKey>> GetTokenKeys()
    {
        return _db.TokenKeys.ToListAsync();
    }

    public async Task<string> GetUserIdByAliasAsync(string alias)
    {
        var res = await _db.Aliases.Where(a => a.Alias == alias).Select(a => a.UserId).FirstOrDefaultAsync();
        return res;
    }

    public async Task<int> GetUsersCount()
    {
        var res = await _db.Credentials.Select(c => c.UserId).Distinct().CountAsync();

        return res;
    }

    public async Task<bool> HasUsersAsync()
    {
        var res = await _db.Credentials.AnyAsync();
        return res;
    }

    public async Task DeleteUser(string userId)
    {
        await _db.Credentials.Where(c => c.UserId == userId).ExecuteDeleteAsync();
        await _db.Aliases.Where(c => c.UserId == userId).ExecuteDeleteAsync();
    }

    public Task<List<ApiKeyDesc>> GetAllApiKeys()
    {
        return _db.ApiKeys.ToListAsync();
    }

    public async Task SetAppDeletionDate(DateTime? deletionAt)
    {
        await _db.AccountInfo.ExecuteUpdateAsync(x => x.SetProperty(a => a.DeleteAt, deletionAt));
    }

    public async Task<bool> CheckIfAliasIsAvailable(IEnumerable<string> aliases, string userId)
    {
        return !await _db.Aliases.AnyAsync(a => aliases.Contains(a.Alias) && a.UserId != userId);
    }

    public async Task SetFeaturesAsync(SetFeaturesRequest features) =>
        await _db.AppFeatures.ExecuteUpdateAsync(x => x
            .SetProperty(f => f.IsGenerateSignInTokenEndpointEnabled,
                existing => features.EnableManuallyGeneratedAuthenticationTokens ??
                            existing.IsGenerateSignInTokenEndpointEnabled
            )
            .SetProperty(f => f.IsMagicLinksEnabled,
                existing => features.EnableMagicLinks ?? existing.IsMagicLinksEnabled
            )
            .SetProperty(f => f.EventLoggingRetentionPeriod,
                existing => features.EventLoggingRetentionPeriod ?? existing.EventLoggingRetentionPeriod
            )
        );

    public async Task SetFeaturesAsync(ManageFeaturesRequest features)
    {
        var existingEntity = await _db.AppFeatures.FirstOrDefaultAsync();
        existingEntity.EventLoggingIsEnabled = features.EventLoggingIsEnabled;
        existingEntity.EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod;
        existingEntity.MagicLinkEmailMonthlyQuota = features.MagicLinkEmailMonthlyQuota;
        existingEntity.MaxUsers = features.MaxUsers;
        existingEntity.AllowAttestation = features.AllowAttestation;
        await _db.SaveChangesAsync();
    }

    public async Task LockApiKeyAsync(string apiKeyId)
    {
        var rows = await _db.ApiKeys
            .Where(x => x.Id == apiKeyId)
            .ExecuteUpdateAsync(apiKey => apiKey
                .SetProperty(x => x.IsLocked, true)
                .SetProperty(x => x.LastLockedAt, _timeProvider.GetUtcNow().UtcDateTime)
        );
        if (rows == 0)
        {
            throw new ArgumentException("ApiKey not found");
        }
    }

    public async Task UnlockApiKeyAsync(string apiKeyId)
    {
        var rows = await _db.ApiKeys
            .Where(x => x.Id == apiKeyId)
            .ExecuteUpdateAsync(apiKey => apiKey
                .SetProperty(x => x.IsLocked, false)
                .SetProperty(x => x.LastUnlockedAt, _timeProvider.GetUtcNow().UtcDateTime)
            );
        if (rows == 0)
        {
            throw new ArgumentException("ApiKey not found");
        }
    }

    public async Task DeleteApiKeyAsync(string apiKeyId)
    {
        var rows = await _db.ApiKeys
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
        var query = _db.PeriodicCredentialReports.AsQueryable();
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
        var query = _db.PeriodicActiveUserReports.AsQueryable();
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
        IQueryable<Authenticator> query = _db.Authenticators;

        if (isAllowed.HasValue)
        {
            query = query.Where(x => x.IsAllowed == isAllowed.Value);
        }

        return await query.ToListAsync();
    }

    public async Task AddAuthenticatorsAsync(IEnumerable<Guid> aaGuids, bool isAllowed)
    {
        var existingAuthenticators = _db.Authenticators.Where(x => aaGuids.Contains(x.AaGuid));

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
                CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
                Tenant = _tenantProvider.Tenant
            }).ToList();

        _db.Authenticators.AddRange(newAuthenticators);
        await _db.SaveChangesAsync();
    }

    public Task RemoveAuthenticatorsAsync(IEnumerable<Guid> aaGuids)
    {
        return _db.Authenticators
            .Where(x => aaGuids.Contains(x.AaGuid))
            .ExecuteDeleteAsync();
    }

    public async Task<IReadOnlyList<DispatchedEmail>> GetDispatchedEmailsAsync(TimeSpan window)
    {
        var from = _timeProvider.GetUtcNow().UtcDateTime - window;
        return await _db.DispatchedEmails
            .Where(x => x.CreatedAt >= from)
            .ToArrayAsync();
    }

    public async Task<int> GetDispatchedEmailCountAsync(TimeSpan window)
    {
        var from = _timeProvider.GetUtcNow().UtcDateTime - window;
        return await _db.DispatchedEmails
            .CountAsync(x => x.CreatedAt >= from);
    }

    public async Task<DispatchedEmail> AddDispatchedEmailAsync(string userId, string emailAddress, string linkTemplate)
    {
        var email = new DispatchedEmail
        {
            Tenant = Tenant,
            Id = Guid.NewGuid(),
            CreatedAt = _timeProvider.GetUtcNow(),
            UserId = userId,
            EmailAddress = emailAddress,
            LinkTemplate = linkTemplate
        };

        _db.DispatchedEmails.Add(email);
        await _db.SaveChangesAsync();

        return email;
    }

    public async Task DeleteOldDispatchedEmailsAsync(TimeSpan age)
    {
        var until = _timeProvider.GetUtcNow().UtcDateTime - age;
        await _db.DispatchedEmails
            .Where(x => x.CreatedAt < until)
            .ExecuteDeleteAsync();

        await _db.SaveChangesAsync();
    }

    public async Task LockAllApiKeys(bool isLocked)
    {
        await _db.ApiKeys.ExecuteUpdateAsync(x => x
            .SetProperty(k => k.IsLocked, isLocked)
            .SetProperty(k => isLocked ? k.LastLockedAt : k.LastUnlockedAt, DateTime.UtcNow)
        );
    }

    public Task RemoveTokenKey(int keyId)
    {
        return _db.TokenKeys.Where(k => k.KeyId == keyId).ExecuteDeleteAsync();
    }

    public Task RemoveExpiredTokenKeys(CancellationToken cancellationToken)
    {
        return _db.TokenKeys.Where(x => x.CreatedAt < _timeProvider.GetUtcNow().AddDays(-30).DateTime)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task SaveAccountInformation(AccountMetaInformation info)
    {
        _db.AccountInfo.Add(info);
        await _db.SaveChangesAsync();
    }

    public async Task StoreAlias(string userid, Dictionary<string, string> aliases)
    {
        var pointers = aliases.Select(a => new AliasPointer() { Tenant = _tenantProvider.Tenant, UserId = userid, Alias = a.Key, Plaintext = a.Value });
        _db.Aliases.RemoveRange(_db.Aliases.Where(ap => ap.UserId == userid));
        _db.Aliases.AddRange(pointers);
        await _db.SaveChangesAsync();
    }

    public async Task StoreApiKey(string pkpart, string apikey, string[] scopes)
    {
        var ak = new ApiKeyDesc
        {
            Tenant = _tenantProvider.Tenant,
            Id = pkpart,
            ApiKey = apikey,
            Scopes = scopes,
            IsLocked = false
        };
        _db.ApiKeys.Add(ak);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> TenantExists()
    {
        return (await _db.AccountInfo.FirstOrDefaultAsync()) != null;
    }

    public async Task UpdateCredential(byte[] credentialId, uint counter, string country, string device)
    {
        var c = await _db.Credentials.Where(c => c.DescriptorId == credentialId).FirstOrDefaultAsync();
        c.SignatureCounter = counter;
        c.Country = country;
        c.Device = device;
        c.LastUsedAt = DateTime.UtcNow;
        _db.Credentials.Update(c);
        await _db.SaveChangesAsync();
    }

    public async Task<List<UserSummary>> GetUsers(string lastUserId)
    {
        var credentialsPerUser = await _db.Credentials
            .OrderBy(c => c.CreatedAt)
            .GroupBy(c => c.UserId)
            .Select((g) =>
                new { UserId = g.Key, LastUsedAt = g.Max(c => c.LastUsedAt), Count = g.Count() })
            .Take(1000)
            .ToListAsync();

        var aliasesPerUser = await _db.Aliases
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