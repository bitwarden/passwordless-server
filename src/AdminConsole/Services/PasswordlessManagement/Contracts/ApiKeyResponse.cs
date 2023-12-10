namespace Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

public record ApiKeyResponse(
    string Id,
    string ApiKey,
    ApiKeyTypes Type,
    HashSet<string> Scopes,
    bool IsLocked,
    DateTime? LastLockedAt);