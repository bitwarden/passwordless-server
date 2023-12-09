namespace Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

public record CreateApiKeyRequest(
    ApiKeyTypes Type,
    HashSet<string> Scopes);