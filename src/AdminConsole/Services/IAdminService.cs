namespace Passwordless.AdminConsole.Services;

public interface IAdminService
{
    Task<bool> CanDisableMagicLinksAsync();
}