using Passwordless.AdminConsole.EventLog.Loggers;

namespace Passwordless.AdminConsole.EventLog;

public class EventDeletionBackgroundWorker : BackgroundService
{
    private readonly ILogger<EventDeletionBackgroundWorker> _logger;
    private readonly IInternalEventStorage _internalEventStorage;

    public EventDeletionBackgroundWorker(
        ILogger<EventDeletionBackgroundWorker> logger,
        IInternalEventStorage internalEventStorage)
    {
        _logger = logger;
        _internalEventStorage = internalEventStorage;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Event Log Deletion Worker");

        using PeriodicTimer timer = new(TimeSpan.FromDays(1));

        try
        {
            do
            {
                await _internalEventStorage.DeleteExpiredEvents(stoppingToken);
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