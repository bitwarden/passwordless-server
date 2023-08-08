using AdminConsole.Db;
using AdminConsole.Models;
using Microsoft.EntityFrameworkCore;

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
            var client = scope.ServiceProvider.GetRequiredService<PasswordlessManagementClient>();
            var db = scope.ServiceProvider.GetRequiredService<ConsoleDbContext>();
            var applicationIds = await client.GetApplicationsPendingDeletion();
            foreach (var applicationId in applicationIds)
            {
                await client.DeleteApplicationAsync(applicationId);
                var application = new Application { Id = applicationId };
                db.Entry(application).State = EntityState.Deleted;
                db.Applications.Remove(application);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to delete applications: {error}", e.Message);
        }
    }
}