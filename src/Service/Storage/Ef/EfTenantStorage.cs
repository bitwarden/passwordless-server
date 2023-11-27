using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class EfTenantStorage : ITenantStorage
{
    private readonly DbTenantContext db;

    public string Tenant { get; }
    public TimeProvider TimeProvider { get; }

    public EfTenantStorage(DbTenantContext db)
    {
        this.db = db;
        Tenant = db.Tenant;
        TimeProvider = db.TimeProvider;
    }

    public async Task AddCredentialToUser(Fido2User user, StoredCredential cred)
    {
        db.Credentials.Add(EFStoredCredential.FromStoredCredential(cred, Tenant));
        await db.SaveChangesAsync();
    }

    public async Task AddTokenKey(TokenKey tokenKey)
    {
        tokenKey.Tenant = Tenant;
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

    public Task<ApiKeyDesc> GetApiKeyAsync(string apiKey)
    {
        var pk = apiKey.Substring(apiKey.Length - 4);
        return db.ApiKeys.FirstOrDefaultAsync(e => e.Id == pk);
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

    public async Task SetFeaturesAsync(SetFeaturesDto features)
    {
        var existingEntity = await db.AppFeatures.FirstOrDefaultAsync();
        existingEntity.EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod;
        await db.SaveChangesAsync();
    }

    public async Task SetFeaturesAsync(ManageFeaturesDto features)
    {
        var existingEntity = await db.AppFeatures.FirstOrDefaultAsync();
        existingEntity.EventLoggingIsEnabled = features.EventLoggingIsEnabled;
        existingEntity.EventLoggingRetentionPeriod = features.EventLoggingRetentionPeriod;
        await db.SaveChangesAsync();
    }

    public async Task SetAppDeletionDate(DateTime deletionAt)
    {
        await db.AccountInfo.ExecuteUpdateAsync(x => x.SetProperty(a => a.DeleteAt, deletionAt));
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
        return db.TokenKeys.Where(x => (TimeProvider.GetUtcNow().DateTime - x.CreatedAt).TotalDays > 30)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task SaveAccountInformation(AccountMetaInformation info)
    {
        db.AccountInfo.Add(info);
        await db.SaveChangesAsync();
    }

    public async Task StoreAlias(string userid, Dictionary<string, string> aliases)
    {
        var pointers = aliases.Select(a => new AliasPointer() { Tenant = Tenant, UserId = userid, Alias = a.Key, Plaintext = a.Value });
        db.Aliases.RemoveRange(db.Aliases.Where(ap => ap.UserId == userid));
        db.Aliases.AddRange(pointers);
        await db.SaveChangesAsync();
    }

    public async Task StoreApiKey(string pkpart, string apikey, string[] scopes)
    {
        var ak = new ApiKeyDesc
        {
            Tenant = Tenant,
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
            new
            {
                UserId = g.Key,
                LastUsedAt = g.Max(c => c.LastUsedAt),
                Count = g.Count()
            })
            .Take(1000)
            .ToListAsync();

        var aliasesPerUser = await db.Aliases
            .GroupBy(a => a.UserId)
            .Select((g) =>
            new
            {
                UserId = g.Key,
                Count = g.Count(),
                Aliases = g.Select(a => a.Plaintext)
            })
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