using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.MDS;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public interface IPasswordlessManagementClient
{
    Task<CreateAppResultDto> CreateApplicationAsync(CreateAppDto options);
    Task<bool> CanDeleteApplicationImmediatelyAsync(string appId);
    Task<MarkDeleteApplicationResponse> MarkDeleteApplicationAsync(MarkDeleteApplicationRequest request);
    Task<bool> DeleteApplicationAsync(string appId);
    Task<CancelApplicationDeletionResponse> CancelApplicationDeletionAsync(string appId);
    Task<ICollection<string>> ListApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(string appId, ManageFeaturesRequest request);
    Task<AppFeatureResponse> GetFeaturesAsync(string appId);
    Task<ICollection<ApiKeyResponse>> GetApiKeysAsync(string appId);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreatePublicKeyRequest request);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreateSecretKeyRequest request);
    Task LockApiKeyAsync(string appId, string apiKeyId);
    Task UnlockApiKeyAsync(string appId, string apiKeyId);
    Task DeleteApiKeyAsync(string appId, string apiKeyId);

    /// <summary>
    /// Retrieve a list of all attestation types in the FIDO2 MDS.
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyCollection<string>> GetAttestationTypesAsync();

    /// <summary>
    /// Retrieve a list of all certification statuses in the FIDO2 MDS.
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyCollection<string>> GetCertificationStatusesAsync();

    /// <summary>
    /// Get a list of all authenticators in the FIDO2 MDS.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<EntryResponse>> GetMetaDataStatementEntriesAsync(EntriesRequest request);
}