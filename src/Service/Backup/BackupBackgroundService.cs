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
    private readonly ILogger<BackupBackgroundService> _logger;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(15));

    public BackupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BackupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting {BackgroundService}.", GetType().Name);
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

                var worker = scope.ServiceProvider.GetRequiredService<IBackupWorkerService>();
                await worker.BackupAsync(pendingJob.Id);

                _logger.LogInformation("{BackgroundService}: {Job} completed.", GetType().Name, pendingJob.Id);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "{BackgroundService}: {Job} failed.", GetType().Name, pendingJob?.Id);
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken));
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping {BackgroundService}.", GetType().Name);
        _timer.Dispose();
        return base.StopAsync(cancellationToken);
    }
}