namespace Passwordless.Service.Models;

public record ValidatePublicKeyDto(string ApplicationId, IReadOnlyCollection<string> Scopes);