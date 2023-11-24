namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public interface IAuthenticatorDataService
{
    Task AddOrUpdateAuthenticatorDataAsync(Guid aaGuid, string name, string icon);
}