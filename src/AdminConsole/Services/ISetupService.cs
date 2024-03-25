namespace Passwordless.AdminConsole.Services;

public interface ISetupService
{
    Task<bool> HasSetupCompletedAsync();
}