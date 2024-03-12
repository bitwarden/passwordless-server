namespace Passwordless.Service.Backup;

public interface IBackupWorkerService
{

    /// <summary>
    /// Creates a new backup.
    /// </summary>
    /// <param name="id">The identifier of the backup job.</param>
    Task BackupAsync(Guid id);
}