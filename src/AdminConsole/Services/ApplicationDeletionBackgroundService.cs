using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public sealed class ApplicationDeletionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationDeletionBackgroundService> _logger;

    public ApplicationDeletionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ApplicationDeletionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");
        using PeriodicTimer timer = new(TimeSpan.FromHours(1));
        try
        {
            await DoWorkAsync();
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Timed Hosted Service failed.");
        }
    }

    private async Task DoWorkAsync()
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IPasswordlessManagementClient>();
            var db = scope.ServiceProvider.GetRequiredService<ConsoleDbContext>();
            var applicationIds = await client.ListApplicationsPendingDeletionAsync();
            foreach (var applicationId in applicationIds)
            {
                if (await client.DeleteApplicationAsync(applicationId))
                {
                    var application = new Application { Id = applicationId };
                    db.Applications.Remove(application);
                    await db.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError("Failed to delete application: {appId}", applicationId);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to delete applications: {error}", e.Message);
        }
    }
}