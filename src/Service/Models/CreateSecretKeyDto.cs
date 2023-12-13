using Passwordless.Common.Constants;

namespace Passwordless.Service.Models;

public record CreateSecretKeyDto(HashSet<SecretKeyScopes> Scopes);