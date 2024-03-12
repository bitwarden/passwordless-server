using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Backup;
using Passwordless.Common.Models.Backup;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Backup;

public class BackupService : IBackupService
{
    private readonly DbTenantContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        DbTenantContext dbContext,
        TimeProvider timeProvider,
        ITenantProvider tenantProvider,
        ILogger<BackupService> logger)
    {
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
}