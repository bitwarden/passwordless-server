namespace Passwordless.Common.Models.Backup;

public record StatusResponse(Guid JobId, JobStatusResponse Status);