using Passwordless.AdminConsole.Services;
using Passwordless.Common.Background;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

/// <summary>
/// Responsible for retrieving the current user count from the Passwordless API and storing it in the Admin Console.
/// </summary>
public sealed class UserCountUpdaterBackgroundService(
    IServiceProvider serviceProvider,
    TimeProvider timeProvider,
    ILogger<UserCountUpdaterBackgroundService> logger)
    : BasePeriodicBackgroundService(
        new TimeOnly(0),
        TimeSpan.FromHours(1),
        timeProvider,
        logger)
{
    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var usageService = scope.ServiceProvider.GetRequiredService<IUsageService>();
            await usageService.UpdateUsersCountAsync();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, $"{nameof(UserCountUpdaterBackgroundService)} failed.");
        }
    }
}