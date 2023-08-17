using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;
using Passwordless.Api.Models;

namespace Passwordless.AdminConsole.Services;

public interface IPasswordlessManagementClient
{
    Task<NewAppResponse> CreateApplication(string appId, NewAppOptions registerOptions);
    Task<MarkDeleteApplicationResponse> MarkDeleteApplication(MarkDeleteApplicationRequest request);
    Task<bool> DeleteApplicationAsync(string application);
    Task<CancelApplicationDeletionResponse> CancelApplicationDeletion(string applicationId);
    Task<ICollection<string>> ListApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(SetApplicationFeaturesRequest request);
}