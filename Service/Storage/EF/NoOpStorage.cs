using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Service.Models;
using Service.Storage;

public class NoOpStorage : IStorage
{
    public Task AddCredentialToUser(Fido2User user, StoredCredential cred)
    {
        throw new NotImplementedException();
    }

    public Task AddTokenKey(TokenKey tokenKey)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAccount()
    {
        throw new NotImplementedException();
    }

    public Task DeleteCredential(byte[] id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(byte[] credentialId)
    {
        throw new NotImplementedException();
    }

    public Task<AccountMetaInformation> GetAccountInformation()
    {
        throw new NotImplementedException();
    }

    public Task<HashSet<string>> GetAliasesByUserId(byte[] userid)
    {
        throw new NotImplementedException();
    }

    public Task<ApiKeyDesc> GetApiKeyAsync(string apiKey)
    {
        throw new NotImplementedException();
    }

    public Task<StoredCredential> GetCredential(byte[] credentialId)
    {
        throw new NotImplementedException();
    }

    public Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias)
    {
        throw new NotImplementedException();
    }

    public Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true)
    {
        throw new NotImplementedException();
    }

    public Task<List<TokenKey>> GetTokenKeys()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetUserIdByAliasAsync(string alias)
    {
        throw new NotImplementedException();
    }

    public Task LockAllApiKeys(bool isLocked)
    {
        throw new NotImplementedException();
    }

    public Task RemoveTokenKey(int keyId)
    {
        throw new NotImplementedException();
    }

    public Task SaveAccountInformation(AccountMetaInformation info)
    {
        throw new NotImplementedException();
    }

    public Task StoreAlias(byte[] userid, HashSet<string> aliases)
    {
        throw new NotImplementedException();
    }

    public Task StoreApiKey(string pkpart, string apikey, string[] scopes)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TenantExists()
    {
        throw new NotImplementedException();
    }

    public Task UpdateCredential(byte[] credentialId, uint counter, string country, string device)
    {
        throw new NotImplementedException();
    }
}
