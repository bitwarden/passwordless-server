using Passwordless.Common.Constants;

namespace Passwordless.Service.Models;

public record CreatePublicKeyDto(HashSet<PublicKeyScopes> Scopes);