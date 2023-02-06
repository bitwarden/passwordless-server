using Fido2NetLib;
using Fido2NetLib.Objects;
using Service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Storage
{
    public interface IStorage
    {
        Task AddCredentialToUser(Fido2User user, StoredCredential cred);
        Task AddTokenKey(TokenKey tokenKey);
        Task DeleteAccount();
        Task DeleteCredential(byte[] id);
        Task<bool> ExistsAsync(byte[] credentialId);
        Task<AccountMetaInformation> GetAccountInformation();
        Task<HashSet<string>> GetAliasesByUserId(byte[] userid);
        Task<ApiKeyDesc> GetApiKeyAsync(string apiKey);
        Task<StoredCredential> GetCredential(byte[] credentialId);
        Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias);
        Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true);
        Task<List<TokenKey>> GetTokenKeys();
        Task<string> GetUserIdByAliasAsync(string alias);
        Task LockAllApiKeys(bool isLocked);
        Task RemoveTokenKey(int keyId);
        Task SaveAccountInformation(AccountMetaInformation info);
        Task StoreAlias(byte[] userid, HashSet<string> aliases);
        Task StoreApiKey(string pkpart, string apikey, string[] scopes);
        Task<bool> TenantExists();
        Task UpdateCredential(byte[] credentialId, uint counter, string country, string device);
    }
}