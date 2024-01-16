using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public interface IPasswordlessManagementClient
{
    Task<CreateAppResultDto> CreateApplication(string appId, CreateAppDto options);
    Task<MarkDeleteApplicationResponse> MarkDeleteApplication(MarkDeleteApplicationRequest request);
    Task<bool> DeleteApplicationAsync(string application);
    Task<CancelApplicationDeletionResponse> CancelApplicationDeletion(string applicationId);
    Task<ICollection<string>> ListApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(string appId, ManageFeaturesRequest request);
    Task<AppFeatureResponse> GetFeaturesAsync(string appId);
    Task<ICollection<ApiKeyResponse>> GetApiKeysAsync(string appId);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreatePublicKeyRequest request);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(string appId, CreateSecretKeyRequest request);
    Task LockApiKeyAsync(string appId, string apiKeyId);
    Task UnlockApiKeyAsync(string appId, string apiKeyId);
    Task DeleteApiKeyAsync(string appId, string apiKeyId);
    Task EnabledManuallyGeneratedTokensAsync(string appId, string performedBy);
    Task DisabledManuallyGeneratedTokensAsync(string appId, string performedBy);
    Task<GetAppIdAvailabilityResponse> IsApplicationIdAvailableAsync(GetAppIdAvailabilityRequest request);
}