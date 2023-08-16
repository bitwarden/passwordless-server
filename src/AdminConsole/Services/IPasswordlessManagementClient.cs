using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.Api.Models;
using Passwordless.Net;

namespace Passwordless.AdminConsole.Services;

public interface IPasswordlessManagementClient
{
    Task<NewAppResponse> CreateApplication(NewAppOptions registerOptions);
    Task<MarkDeleteApplicationResponse> MarkDeleteApplication(MarkDeleteApplicationRequest request);
    Task<bool> DeleteApplicationAsync(string application);
    Task<CancelApplicationDeletionResponse> CancelApplicationDeletion(string applicationId);
    Task<ICollection<string>> ListApplicationsPendingDeletionAsync();
    Task SetFeaturesAsync(SetApplicationFeaturesRequest request);
}