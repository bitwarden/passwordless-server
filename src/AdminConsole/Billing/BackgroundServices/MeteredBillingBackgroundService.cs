using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

/// <summary>
/// Responsible for synchronizing the metered seat usage with our billing partner.
/// </summary>
public sealed class MeteredBillingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MeteredBillingBackgroundService> _logger;

    private readonly TimeSpan _interval = TimeSpan.FromDays(7);

    public MeteredBillingBackgroundService(
        IServiceProvider services,
        ILogger<MeteredBillingBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(MeteredBillingBackgroundService)} is running.");
        using PeriodicTimer timer = new(_interval);
        try
        {
            await SynchronizeAsync();
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SynchronizeAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"{nameof(MeteredBillingBackgroundService)} is stopping.");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"{nameof(MeteredBillingBackgroundService)} failed.");
        }
    }

    private async Task SynchronizeAsync()
    {
        try
        {
            using IServiceScope scope = _services.CreateScope();
            var billingService = scope.ServiceProvider.GetRequiredService<ISharedBillingService>();
            await billingService.UpdateUsageAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to update billing: {error}", e.Message);
        }
    }
}