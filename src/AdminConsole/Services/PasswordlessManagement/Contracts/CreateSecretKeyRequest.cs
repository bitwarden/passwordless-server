using Passwordless.Common.Constants;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

public record CreateSecretKeyRequest(HashSet<SecretKeyScopes> Scopes);