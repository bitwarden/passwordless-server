using Passwordless.AdminConsole.Models;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Services;

public interface IApplicationService
{
    Task<bool> CanDeleteApplicationImmediatelyAsync(string applicationId);
    Task<MarkDeleteApplicationResponse> MarkDeleteApplicationAsync(string applicationId, string userName);
    Task CancelDeletionForApplicationAsync(string applicationId);
    Task<Onboarding?> GetOnboardingAsync(string applicationId);
    Task DeleteAsync(string applicationId);
    Task CreateApplicationAsync(Application application);
}