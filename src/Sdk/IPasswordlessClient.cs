using Passwordless.Net.Models;

namespace Passwordless.Net;

public interface IPasswordlessClient
{
    Task<RegisterTokenResponse> CreateRegisterTokenAsync(RegisterOptions registerOptions, CancellationToken cancellationToken = default);
    Task DeleteCredentialAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteCredentialAsync(byte[] id, CancellationToken cancellationToken = default);
    Task<List<AliasPointer>> ListAliasesAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Credential>> ListCredentialsAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<PasswordlessUserSummary>?> ListUsersAsync(CancellationToken cancellationToken = default);
    Task<VerifiedUser?> VerifyTokenAsync(string verifyToken, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}
