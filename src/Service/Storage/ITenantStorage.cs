using Fido2NetLib;
using Fido2NetLib.Objects;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Storage;

public interface ITenantStorage
{
    string Tenant { get; }
    Task AddCredentialToUser(Fido2User user, StoredCredential cred);
    Task AddTokenKey(TokenKey tokenKey);
    Task DeleteAccount();
    Task DeleteCredential(byte[] id);
    Task<bool> ExistsAsync(byte[] credentialId);
    Task<AccountMetaInformation> GetAccountInformation();
    Task<ApiKeyDesc> GetApiKeyAsync(string apiKey);
    Task<StoredCredential> GetCredential(byte[] credentialId);
    Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias);
    Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true);
    Task<List<StoredCredential>> GetCredentialsByUserIdAsync(string userId);
    Task<List<TokenKey>> GetTokenKeys();
    Task LockAllApiKeys(bool isLocked);
    Task RemoveTokenKey(int keyId);
    Task SaveAccountInformation(AccountMetaInformation info);
    Task StoreApiKey(string pkpart, string apikey, string[] scopes);
    Task<bool> TenantExists();
    Task UpdateCredential(byte[] credentialId, uint counter, string country, string device);
    Task<List<UserSummary>> GetUsers(string lastUserId);

    // Aliases
    Task<List<AliasPointer>> GetAliasesByUserId(string userid);
    Task StoreAlias(string userid, Dictionary<string, string> aliases);

    Task<string> GetUserIdByAliasAsync(string alias);


    Task<int> GetUsersCount();
    Task DeleteUser(string userId);
    Task<List<ApiKeyDesc>> GetAllApiKeys();
    Task SetAppDeletionDate(DateTime? deletionAt);
    Task<bool> CheckIfAliasIsAvailable(IEnumerable<string> aliases, string userId);
}
