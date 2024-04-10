namespace Passwordless.Common.Background;

public abstract class BaseDelayedPeriodicBackgroundService : BackgroundService
{
    protected ILogger Logger;

    /// <summary>
    /// The delay before the first execution.
    /// </summary>
    private readonly TimeOnly _delay;

    /// <summary>
    /// The period of time between executions.
    /// </summary>
    private readonly TimeSpan _period;

    private ITimer? _timer;

    private Task? _executingTask;

    protected readonly TimeProvider TimeProvider;

    /// <summary>
    ///
    /// </summary>
    /// <param name="delay">The delay before the first execution.</param>
    /// <param name="period">The period of time between executions.</param>
    /// <param name="timeProvider"></param>
    /// <param name="logger"></param>
    protected BaseDelayedPeriodicBackgroundService(
        TimeOnly delay,
        TimeSpan period,
        TimeProvider timeProvider,
        ILogger logger)
    {
        _delay = delay;
        _period = period;
        TimeProvider = timeProvider;
        Logger = logger;
    }

    protected CancellationTokenSource CancellationToken { get; } = new();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = _delay.ToTimeSpan();
        Logger.LogInformation("Starting {BackgroundService} in {InitialDelay}.", GetType().Name, delay);
        _timer = new Timer(DoWork, null, delay, _period);

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _executingTask = DoWorkAsync(CancellationToken.Token);
    }

    protected abstract Task DoWorkAsync(CancellationToken cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopping {BackgroundService}.", GetType().Name);

        // Prevent from spawning new workers
        _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        if (_executingTask == null)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        try
        {
            // Signal the worker to cancel
            await CancellationToken.CancelAsync();
        }
        finally
        {
            if (_executingTask != null)
            {
                // Wait until the worker completes or the stop token triggers
                await Task.WhenAny(_executingTask!, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);

        _timer?.Dispose();
        CancellationToken.Dispose();
        Logger.LogInformation("Stopped {BackgroundService}.", GetType().Name);
    }
}