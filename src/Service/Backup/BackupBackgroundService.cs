using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Passwordless.Common.Models.Backup;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Backup;

public class BackupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BackupBackgroundService> _logger;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(15));

    public BackupBackgroundService(
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        ILogger<BackupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            using var scope = _serviceProvider.CreateScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<DbGlobalContext>();
            ArchiveJob? pendingJob = null;
            try
            {
                pendingJob = await dbContext.ArchiveJobs
                    .Where(x => x.Status == JobStatus.Pending)
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (pendingJob == null)
                {
                    continue;
                }

                pendingJob.Status = JobStatus.Running;
                pendingJob.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await dbContext.SaveChangesAsync(stoppingToken);

                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

                await backupService.BackupAsync(pendingJob.Id);

                pendingJob.Status = JobStatus.Completed;
                pendingJob.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            }
            catch (Exception)
            {
                pendingJob!.Status = JobStatus.Failed;
                pendingJob.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            }
            finally
            {
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken));
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        return base.StopAsync(cancellationToken);
    }
}