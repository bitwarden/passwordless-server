using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Backup;
using Passwordless.Common.Models.Backup;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Backup;

public class BackupWorkerService : IBackupWorkerService
{
    private readonly IBackupSerializer _backupSerializer;
    private readonly DbGlobalContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BackupService> _logger;

    public BackupWorkerService(
        IBackupSerializer backupSerializer,
        DbGlobalContext dbContext,
        TimeProvider timeProvider,
        ILogger<BackupService> logger)
    {
        _backupSerializer = backupSerializer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Backup the data for the given job.
    /// </summary>
    /// <param name="id">The identifier of the backup job.</param>
    /// <exception cref="ApiException">Thrown when the job is not found.</exception>
    public async Task BackupAsync(Guid id)
    {
        var job = await _dbContext.ArchiveJobs.FirstOrDefaultAsync(x => x.Id == id);
        if (job == null) throw new ApiException("Job not found", 404);

        job.Status = JobStatus.Running;
        job.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _dbContext.SaveChangesAsync();

        try
        {
            await BackupEntitiesAsync(job);

            job.Status = JobStatus.Completed;
            job.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed backup job {Id}", id);

            job.Status = JobStatus.Failed;
            job.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task BackupEntitiesAsync(ArchiveJob job)
    {
        await BackupEntityAsync<AccountMetaInformation>(job.Id, job.Tenant);
        await BackupEntityAsync<ApiKeyDesc>(job.Id, job.Tenant);
        await BackupEntityAsync<AppFeature>(job.Id, job.Tenant);
        await BackupEntityAsync<Authenticator>(job.Id, job.Tenant);
        await BackupEntityAsync<AliasPointer>(job.Id, job.Tenant);
        await BackupEntityAsync<EFStoredCredential>(job.Id, job.Tenant);
        await BackupEntityAsync<ApplicationEvent>(job.Id, job.Tenant);
        await BackupEntityAsync<PeriodicCredentialReport>(job.Id, job.Tenant);
        await BackupEntityAsync<PeriodicActiveUserReport>(job.Id, job.Tenant);
    }

    private async Task BackupEntityAsync<TEntity>(Guid groupId, string tenant) where TEntity : PerTenant
    {
        var entities = await _dbContext.Set<TEntity>().Where(x => x.Tenant == tenant).ToListAsync();
        var data = _backupSerializer.Serialize(entities);

        var archive = new Archive
        {
            Id = Guid.NewGuid(),
            JobId = groupId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Entity = typeof(TEntity),
            Data = data,
            Tenant = tenant
        };

        _dbContext.Archives.Add(archive);
        await _dbContext.SaveChangesAsync();
    }
}