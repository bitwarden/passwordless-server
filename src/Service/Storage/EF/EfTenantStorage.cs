using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class EfTenantStorage : ITenantStorage
{
    private readonly DbTenantContext db;
    private readonly string _tenant;

    public EfTenantStorage(DbTenantContext db)
    {
        this.db = db;
        _tenant = db.Tenant;
    }

    public async Task AddCredentialToUser(Fido2User user, StoredCredential cred)
    {
        db.Credentials.Add(EFStoredCredential.FromStoredCredential(cred, _tenant));
        await db.SaveChangesAsync();
    }

    public async Task AddTokenKey(TokenKey tokenKey)
    {
        tokenKey.Tenant = _tenant;
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

        var pkcred = descs.Select(x => new PublicKeyCredentialDescriptor() { Id = x.DescriptorId, Transports = x.DescriptorTransports, Type = x.DescriptorType }).ToList();
        return pkcred;
    }

    public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true)
    {
        // TODO: Returning a c.descriptor does not work.
        var res = await db.Credentials.Where(c => c.UserId == userId).ToListAsync();

        var pkcred = res.Select(x => new PublicKeyCredentialDescriptor() { Id = x.DescriptorId, Transports = x.DescriptorTransports, Type = x.DescriptorType }).ToList();

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

    public async Task SaveAccountInformation(AccountMetaInformation info)
    {
        db.AccountInfo.Add(info);
        await db.SaveChangesAsync();
    }

    public async Task StoreAlias(string userid, Dictionary<string, string> aliases)
    {
        var pointers = aliases.Select(a => new AliasPointer() { Tenant = _tenant, UserId = userid, Alias = a.Key, Plaintext = a.Value });
        db.Aliases.RemoveRange(db.Aliases.Where(ap => ap.UserId == userid));
        db.Aliases.AddRange(pointers);
        await db.SaveChangesAsync();
    }

    public async Task StoreApiKey(string pkpart, string apikey, string[] scopes)
    {
        var ak = new ApiKeyDesc
        {
            Tenant = _tenant,
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
        // TODO: Support mor information by counting credentials etc.
        var res = await db
            .Credentials
            .OrderBy(c => c.CreatedAt)
            .GroupJoin(db.Aliases,
                a => a.UserId,
                b => b.UserId,
                (a, aliasCollection) => new
                {
                    UserId = a.UserId,
                    DescriptorId = a.DescriptorId,
                    LastUsedAt = (DateTime?)a.LastUsedAt ?? default(DateTime),
                    Aliases = aliasCollection.DefaultIfEmpty().Select(x => x),
                })
            .DefaultIfEmpty()
            .Take(100)
            .GroupBy(x => x.UserId)
            .ToListAsync();



        var m = res.Where(c => c.Key != null).Select(c => new UserSummary
        {
            UserId = c.Key,
            Aliases = c.SelectMany(x => x.Aliases.Select(a => a.Plaintext)).ToList(),
            AliasCount = c.SelectMany(x => x.Aliases).DistinctBy<AliasPointer, string>(o => o.Alias).Count(),
            CredentialsCount = c.Where(x => x.DescriptorId != null).Select(x => Encoding.UTF8.GetString(x.DescriptorId)).Distinct().Count(),
            LastUsedAt = c.Max(c => c.LastUsedAt)
        });

        return m.ToList();

    }
}