using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public interface IDataService
{
    Task<List<Application>> GetApplications();
    Task<Organization> GetOrganization();
    Task<bool> AllowedToCreateApplication();
    Task<bool> CanInviteAdmin();
    Task<Organization> GetOrganizationWithData();
    Task<List<ConsoleAdmin>> GetConsoleAdmins();
    Task<ConsoleAdmin> GetUserAsync();
    Task<bool> DeleteOrganizationAsync();
    Task<Application?> GetApplicationAsync(string applicationId);
    Task<bool> CanConnectAsync();
    Task CleanUpOnboardingAsync();
}