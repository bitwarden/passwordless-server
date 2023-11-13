using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

/// <summary>
/// Responsible for retrieving the current user count from the Passwordless API and storing it in the Admin Console.
/// </summary>
public sealed class UserCountUpdaterBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<UserCountUpdaterBackgroundService> _logger;

    public UserCountUpdaterBackgroundService(
        IServiceProvider services,
        ILogger<UserCountUpdaterBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(UserCountUpdaterBackgroundService)} is running.");
        using PeriodicTimer timer = new(TimeSpan.FromHours(1));
        try
        {
            await UpdateAsync();
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"{nameof(UserCountUpdaterBackgroundService)} is stopping.");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"{nameof(UserCountUpdaterBackgroundService)} failed.");
        }
    }

    private async Task UpdateAsync()
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
}