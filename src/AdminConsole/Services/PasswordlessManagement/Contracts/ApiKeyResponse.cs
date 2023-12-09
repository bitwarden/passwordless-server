namespace Passwordless.AdminConsole.Services.PasswordlessManagement.Contracts;

public record ApiKeyResponse(
    string ApiKey,
    ApiKeyTypes Type,
    HashSet<string> Scopes,
    bool IsLocked,
    DateTime? LastLockedAt,
    DateTime? LastUnlockedAt);