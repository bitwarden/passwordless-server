using Passwordless.Common.Constants;

namespace Passwordless.Common.Models.Apps;

public record CreateSecretKeyRequest(HashSet<SecretKeyScopes> Scopes);