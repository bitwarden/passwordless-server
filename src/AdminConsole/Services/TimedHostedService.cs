namespace Passwordless.AdminConsole.Services;

public class TimedHostedService : BackgroundService
{
    private readonly ILogger<TimedHostedService> _logger;
    private readonly IServiceProvider _services;

    public TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");
        using PeriodicTimer timer = new(TimeSpan.FromHours(1));
        try
        {
            await DoWork();
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork();
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

    // Could also be a async method, that can be awaited in ExecuteAsync above
    private async Task DoWork()
    {
        await UpdateUsageStatistics();
        await UpdateBilling();
        await CleanUpOnboarding();
    }

    private async Task UpdateUsageStatistics()
    {
        try
        {
            using IServiceScope scope = _services.CreateScope();
            var usageService = scope.ServiceProvider.GetRequiredService<IUsageService>();
            await usageService.UpdateUsersCountAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to update usage statistics: {error}", e.Message);
        }
    }

    private async Task UpdateBilling()
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

    private async Task CleanUpOnboarding()
    {
        try
        {
            using IServiceScope scope = _services.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
            await dataService.CleanUpOnboardingAsync();
            _logger.LogInformation("Cleaned up onboarding data");
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to clean up onboarding data: {error}", e.Message);
        }
    }
}