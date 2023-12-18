using Passwordless.AdminConsole.Models;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services;

public interface IApplicationService
{
    Task<MarkDeleteApplicationResponse> MarkApplicationForDeletionAsync(string applicationId, string userName);
    Task CancelDeletionForApplicationAsync(string applicationId);
    Task<Onboarding?> GetOnboardingAsync(string applicationId);
    Task DeleteAsync(string applicationId);
    Task CreateApplicationAsync(Application application);
}