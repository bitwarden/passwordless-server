using Passwordless.Common.Models.Backup;

namespace Passwordless.Service.Backup;

public interface IBackupService
{
    /// <summary>
    /// Schedules a new backup to be created.
    /// </summary>
    /// <returns></returns>
    Task<Guid> ScheduleAsync();

    /// <summary>
    /// Retrieves all the background jobs.
    /// </summary>
    /// <returns></returns>
    Task<IReadOnlyCollection<StatusResponse>> GetJobsAsync();
}