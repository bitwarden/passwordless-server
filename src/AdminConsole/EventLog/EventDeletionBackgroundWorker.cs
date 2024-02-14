using Passwordless.AdminConsole.EventLog.Loggers;

namespace Passwordless.AdminConsole.EventLog;

public class EventDeletionBackgroundWorker : BackgroundService
{
    private readonly ILogger<EventDeletionBackgroundWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public EventDeletionBackgroundWorker(
        ILogger<EventDeletionBackgroundWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Event Log Deletion Worker");

        using PeriodicTimer timer = new(TimeSpan.FromDays(1));

        try
        {
            do
            {
                using var scope = _serviceProvider.CreateScope();
                var eventLogStorageContext = scope.ServiceProvider.GetRequiredService<IInternalEventLogStorageContext>();
                await eventLogStorageContext.DeleteExpiredEvents(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Event Log Deletion Worker was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event Log Deletion failed.");
        }
    }
}