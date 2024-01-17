namespace Passwordless.Common.Models.Apps;

public record ApiKeyResponse(
    string Id,
    DateTime CreatedAt,
    string ApiKey,
    ApiKeyTypes Type,
    HashSet<string> Scopes,
    bool IsLocked,
    DateTime? LastLockedAt);