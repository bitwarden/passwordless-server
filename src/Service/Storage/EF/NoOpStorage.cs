using Fido2NetLib;
using Fido2NetLib.Objects;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public class NoOpStorage : ITenantStorage
{
    public string Tenant => null!;
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

    public Task<List<AliasPointer>> GetAliasesByUserId(string userid)
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

    public Task<List<StoredCredential>> GetCredentialsByUserIdAsync(string userId)
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

    public Task<int> GetUsersCount()
    {
        throw new NotImplementedException();
    }

    public Task DeleteUser(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<ApiKeyDesc>> GetAllApiKeys()
    {
        throw new NotImplementedException();
    }

    public Task SetAppDeletionDate(DateTime? deletionAt)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckIfAliasIsAvailable(IEnumerable<string> aliases, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<UserSummary>> GetUsers(string lastUserId)
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

    public Task StoreAlias(string userid, Dictionary<string, string> aliases)
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
