namespace Passwordless.Common.Backup.Contracts;

public record Archive(
    Guid Id,
    Guid GroupId,
    DateTime CreatedAt,
    string Entity,
    string Content);