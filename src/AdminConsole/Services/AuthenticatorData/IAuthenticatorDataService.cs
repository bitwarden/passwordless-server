using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public interface IAuthenticatorDataService
{
    Task AddOrUpdateAuthenticatorDataAsync(Guid aaGuid, string name, string icon);
    Task<IEnumerable<Authenticator>> GetAsync(IEnumerable<Guid> aaGuids);
}