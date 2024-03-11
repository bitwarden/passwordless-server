namespace Passwordless.Common.Models.Backup;

/// <param name="JobId">The identifier of the backup job.</param>
public record ScheduleBackupResponse(Guid JobId);