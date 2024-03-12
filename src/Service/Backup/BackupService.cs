using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Backup;
using Passwordless.Common.Models.Backup;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Backup;

public class BackupService : IBackupService
{
    private readonly IBackupSerializer _backupSerializer;
    private readonly DbTenantContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        IBackupSerializer backupSerializer,
        DbTenantContext dbContext,
        TimeProvider timeProvider,
        ITenantProvider tenantProvider,
        ILogger<BackupService> logger)
    {
        _backupSerializer = backupSerializer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    /// <summary>
    /// Schedules a new backup to be created.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ApiException">Thrown when there is already a pending job.</exception>
    public async Task<Guid> ScheduleAsync()
    {
        if (await _dbContext.ArchiveJobs.AnyAsync(x => x.Status == JobStatus.Pending || x.Status == JobStatus.Running))
        {
            throw new ApiException("There is already a pending job.", 400);
        }

        var id = Guid.NewGuid();
        _dbContext.ArchiveJobs.Add(new ArchiveJob
        {
            Id = id,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Status = JobStatus.Pending,
            Tenant = _tenantProvider.Tenant
        });
        await _dbContext.SaveChangesAsync();
        _logger.LogWarning("Scheduled backup job with id {Id}", id);
        return id;
    }

    /// <summary>
    /// Retrieves all the background jobs.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ApiException">Thrown when the job is not found.</exception>
    public async Task<IReadOnlyCollection<StatusResponse>> GetJobsAsync()
    {
        var jobs = await _dbContext.ArchiveJobs
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        if (jobs == null)
        {
            throw new ApiException("Job not found", 404);
        }

        var result = new List<StatusResponse>(jobs.Count);
        foreach (var job in jobs)
        {
            var statusDto = job.Status switch
            {
                JobStatus.Completed => JobStatusResponse.Completed,
                JobStatus.Failed => JobStatusResponse.Failed,
                JobStatus.Pending => JobStatusResponse.Pending,
                JobStatus.Running => JobStatusResponse.Running,
                _ => throw new ArgumentOutOfRangeException()
            };
            result.Add(new StatusResponse(job.Id, job.CreatedAt, statusDto, job.UpdatedAt));
        }

        return result;
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
        await BackupEntityAsync<AccountMetaInformation>(job.Id);
        await BackupEntityAsync<ApiKeyDesc>(job.Id);
        await BackupEntityAsync<AppFeature>(job.Id);
        await BackupEntityAsync<Authenticator>(job.Id);
        await BackupEntityAsync<AliasPointer>(job.Id);
        await BackupEntityAsync<EFStoredCredential>(job.Id);
        await BackupEntityAsync<ApplicationEvent>(job.Id);
        await BackupEntityAsync<PeriodicCredentialReport>(job.Id);
        await BackupEntityAsync<PeriodicActiveUserReport>(job.Id);
    }

    private async Task BackupEntityAsync<TEntity>(Guid groupId) where TEntity : class
    {
        var entities = await _dbContext.Set<TEntity>().ToListAsync();
        var data = _backupSerializer.Serialize(entities);

        var archive = new Archive
        {
            Id = Guid.NewGuid(),
            JobId = groupId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Entity = typeof(TEntity),
            Data = data,
            Tenant = _tenantProvider.Tenant
        };

        _dbContext.Archives.Add(archive);
        await _dbContext.SaveChangesAsync();
    }
}