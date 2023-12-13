namespace Passwordless.Service.Models;

public record ValidateSecretKeyDto(string ApplicationId, IReadOnlyCollection<string> Scopes);