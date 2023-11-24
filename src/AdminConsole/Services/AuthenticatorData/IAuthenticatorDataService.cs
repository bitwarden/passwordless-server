using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public interface IAuthenticatorDataService
{
    Task AddOrUpdateAuthenticatorDataAsync(Guid aaGuid, string name);
    Task<IEnumerable<Authenticator>> GetAsync(IEnumerable<Guid> aaGuids);
}