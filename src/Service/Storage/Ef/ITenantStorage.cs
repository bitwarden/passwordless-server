﻿using Fido2NetLib;
using Fido2NetLib.Objects;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;

namespace Passwordless.Service.Storage.Ef;

public interface ITenantStorage
{
    string Tenant { get; }
    Task<ApiKeyDesc> GetApiKeyAsync(string apiKey);
    Task AddCredentialToUser(Fido2User user, StoredCredential cred);
    Task AddTokenKey(TokenKey tokenKey);
    Task DeleteAccount();
    Task DeleteCredential(byte[] id);
    Task<bool> ExistsAsync(byte[] credentialId);
    Task<AccountMetaInformation> GetAccountInformation();
    Task<AppFeature> GetAppFeaturesAsync();
    Task<StoredCredential> GetCredential(byte[] credentialId);
    Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias);
    Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true);
    Task<List<StoredCredential>> GetCredentialsByUserIdAsync(string userId);
    Task<List<TokenKey>> GetTokenKeys();
    Task LockAllApiKeys(bool isLocked);
    Task RemoveTokenKey(int keyId);
    Task RemoveExpiredTokenKeys(CancellationToken cancellationToken);
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
    Task<bool> HasUsersAsync();
    Task DeleteUser(string userId);
    Task<List<ApiKeyDesc>> GetAllApiKeys();
    Task SetAppDeletionDate(DateTime? deletionAt);
    Task<bool> CheckIfAliasIsAvailable(IEnumerable<string> aliases, string userId);
    Task SetFeaturesAsync(SetFeaturesRequest features);
    Task SetFeaturesAsync(ManageFeaturesRequest features);

    Task LockApiKeyAsync(string apiKeyId);
    Task UnlockApiKeyAsync(string apiKeyId);
    Task DeleteApiKeyAsync(string apiKeyId);
    Task<IEnumerable<PeriodicCredentialReport>> GetPeriodicCredentialReportsAsync(DateOnly? from, DateOnly? to);
    Task<IEnumerable<PeriodicActiveUserReport>> GetPeriodicActiveUserReportsAsync(DateOnly? from, DateOnly? to);
    Task<IReadOnlyCollection<Authenticator>> GetAuthenticatorsAsync(bool? isAllowed = null);
    Task AddAuthenticatorsAsync(IEnumerable<Guid> aaGuids, bool isAllowed);
    Task RemoveAuthenticatorsAsync(IEnumerable<Guid> aaGuids);

    Task<IReadOnlyList<DispatchedEmail>> GetDispatchedEmailsAsync(TimeSpan window);
    Task<int> GetDispatchedEmailCountAsync(TimeSpan window);
    Task<DispatchedEmail> AddDispatchedEmailAsync(string userId, string emailAddress, string linkTemplate);
    Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync(GetAuthenticationConfigurationsFilter filter);
    Task CreateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);
    Task UpdateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);
    Task DeleteAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);
}