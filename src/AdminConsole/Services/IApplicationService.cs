using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Models.DTOs;

namespace Passwordless.AdminConsole.Services;

public interface IApplicationService
{
    Task<MarkDeleteApplicationResponse> MarkApplicationForDeletionAsync(string applicationId, string userName);
    Task CancelDeletionForApplicationAsync(string applicationId);
    Task<Onboarding?> GetOnboardingAsync(string applicationId);
    Task DeleteAsync(string applicationId);
}