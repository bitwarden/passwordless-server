namespace Passwordless.Service.Models;

public record CreateApiKeyDto(
    ApiKeyTypes Type,
    HashSet<string> Scopes);