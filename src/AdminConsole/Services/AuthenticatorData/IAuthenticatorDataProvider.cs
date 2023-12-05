using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services.AuthenticatorData;

public interface IAuthenticatorDataProvider
{
    IReadOnlyCollection<Authenticator> Authenticators { get; }

    string? GetName(Guid aaGuid);
}