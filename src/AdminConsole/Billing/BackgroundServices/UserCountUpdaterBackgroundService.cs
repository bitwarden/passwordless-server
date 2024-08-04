using Passwordless.AdminConsole.Services;
using Passwordless.Common.Background;

namespace Passwordless.AdminConsole.Billing.BackgroundServices;

/// <summary>
/// Responsible for retrieving the current user count from the Passwordless API and storing it in the Admin Console.
/// It is only required for Bitwarden's internal reporting.
/// </summary>
public sealed class UserCountUpdaterBackgroundService(
    IServiceProvider serviceProvider,
    TimeProvider timeProvider,
    ILogger<UserCountUpdaterBackgroundService> logger)
    : BaseDelayedPeriodicBackgroundService(
        new TimeOnly(0, 0),
        TimeSpan.FromDays(1),
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