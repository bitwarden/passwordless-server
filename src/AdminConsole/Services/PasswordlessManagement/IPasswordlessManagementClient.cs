using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public interface IPasswordlessManagementClient
{
    Task<NewAppResponse> CreateApplication(string appId, NewAppOptions registerOptions);
    Task<MarkDeleteApplicationResponse> MarkDeleteApplication(MarkDeleteApplicationRequest request);
    Task<bool> DeleteApplicationAsync(string application);
    Task<CancelApplicationDeletionResponse> CancelApplicationDeletion(string applicationId);
    Task<ICollection<string>> ListApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(string appId, SetApplicationFeaturesRequest request);
    Task<AppFeatureDto> GetFeaturesAsync(string appId);
    Task<ICollection<ApiKeyResponse>> GetApiKeysAsync(string appId);
    Task CreateApiKeyAsync(string appId, CreateApiKeyRequest request);
    Task LockApiKeyAsync(string appId, object apiKeyId);
    Task UnlockApiKeyAsync(string appId, object apiKeyId);
    Task DeleteApiKeyAsync(string appId, object apiKeyId);
}