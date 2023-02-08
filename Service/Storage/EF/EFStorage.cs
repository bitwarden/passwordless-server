using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Service.Models;
using Service.Storage;

public class EFStorage : IStorage
{
    private readonly DbTenantContext db;

    public EFStorage(DbTenantContext db)
    {
        this.db = db;
    }

    public async Task AddCredentialToUser(Fido2User user, StoredCredential cred)
    {
        db.Credentials.Add(EFStoredCredential.FromStoredCredential(cred));
        await db.SaveChangesAsync();
    }

    public async Task AddTokenKey(TokenKey tokenKey)
    {
        db.TokenKeys.Add(tokenKey);
        await db.SaveChangesAsync();
    }

    public Task DeleteAccount()
    {
        throw new NotImplementedException();
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

    public async Task<HashSet<string>> GetAliasesByUserId(byte[] userid)
    {
        var strId = Encoding.UTF8.GetString(userid);
        var res = await db.Aliases.Where(a => a.UserId == strId).ToListAsync();

        var rec = new HashSet<string>(res.Select(p => p.Alias));
        return rec;
    }

    public Task<ApiKeyDesc> GetApiKeyAsync(string apiKey)
    {
        var pk = apiKey.Substring(apiKey.Length - 4);
        return db.ApiKeys.FirstOrDefaultAsync(e => e.Id == pk);
    }

    public async Task<StoredCredential> GetCredential(byte[] credentialId)
    {
        var res = await db.Credentials.FirstOrDefaultAsync(c => c.DescriptorId == credentialId);
        
        return res.ToStoredCredential();
    }

    public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias)
    {
        var aliases = await db.Aliases.FirstOrDefaultAsync(a => a.Alias == alias);
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

    public Task LockAllApiKeys(bool isLocked)
    {
        throw new NotImplementedException();
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

    public async Task StoreAlias(byte[] userid, HashSet<string> aliases)
    {
        var strUserId = Encoding.UTF8.GetString(userid);
        var pointers = aliases.Select(a => new AliasPointer() { UserId = strUserId, Alias = a });
        db.Aliases.RemoveRange(db.Aliases.Where(ap => ap.UserId == strUserId));
        db.Aliases.AddRange(pointers);
        await db.SaveChangesAsync();
    }

    public async Task StoreApiKey(string pkpart, string apikey, string[] scopes)
    {
        var ak = new ApiKeyDesc
        {
            Id = pkpart,
            ApiKey = apikey,
            Scopes = scopes,
            IsLocked = false
        };
        db.ApiKeys.Add(ak);
        await db.SaveChangesAsync();
    }

    public Task<bool> TenantExists()
    {
        return db.Database.CanConnectAsync();
    }


    public async Task UpdateCredential(byte[] credentialId, uint counter, string country, string device)
    {
        var c = await db.Credentials.Where(c => c.DescriptorId == credentialId).FirstOrDefaultAsync();
        c.SignatureCounter = counter;
        c.Country = country;
        c.Device = device;
        db.Credentials.Update(c);
        await db.SaveChangesAsync();
    }
}