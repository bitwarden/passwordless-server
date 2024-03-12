namespace Passwordless.Common.Models.Backup;

public record StatusResponse(Guid JobId, DateTime CreatedAt, JobStatusResponse Status, DateTime? UpdatedAt);