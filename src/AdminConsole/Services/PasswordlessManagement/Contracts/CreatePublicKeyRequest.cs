using Passwordless.Common.Constants;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

public record CreatePublicKeyRequest(HashSet<PublicKeyScopes> Scopes);