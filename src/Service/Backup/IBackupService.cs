using Passwordless.Common.Models.Backup;

namespace Passwordless.Service.Backup;

public interface IBackupService
{
    Task<Guid> ScheduleAsync();
    Task<IReadOnlyCollection<StatusResponse>> GetJobsAsync();
    Task BackupAsync(Guid id);
}