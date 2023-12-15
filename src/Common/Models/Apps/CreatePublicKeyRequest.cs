using Passwordless.Common.Constants;

namespace Passwordless.Common.Models.Apps;

public record CreatePublicKeyRequest(HashSet<PublicKeyScopes> Scopes);