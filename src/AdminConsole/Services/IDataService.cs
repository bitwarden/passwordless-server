using Passwordless.AdminConsole.BackgroundServices;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public interface IDataService
{
    Task<List<Application>> GetApplicationsAsync();
    Task<Organization> GetOrganizationAsync();
    Task<bool> AllowedToCreateApplicationAsync();
    Task<bool> CanInviteAdminAsync();
    Task<Organization> GetOrganizationWithDataAsync();
    Task<List<ConsoleAdmin>> GetConsoleAdminsAsync();
    Task<ConsoleAdmin> GetUserAsync();
    Task<bool> DeleteOrganizationAsync();
    Task<Application?> GetApplicationAsync(string applicationId);
    Task<bool> CanConnectAsync();
    Task CleanUpOnboardingAsync();
    Task CreateOrganizationAsync(Organization organization);
    Task<UnconfirmedAccountCleanUpQueryResult> CleanUpUnconfirmedAccounts(CancellationToken cancellationToken);
}