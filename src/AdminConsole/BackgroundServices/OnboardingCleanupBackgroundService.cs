using Passwordless.AdminConsole.Services;
using Passwordless.Common.Background;

namespace Passwordless.AdminConsole.BackgroundServices;

public class OnboardingCleanupBackgroundService(
    ILogger<OnboardingCleanupBackgroundService> logger,
    TimeProvider timeProvider,
    IServiceProvider services)
    : BasePeriodicBackgroundService(new TimeOnly(0), TimeSpan.FromDays(1), timeProvider, logger)
{
    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = services.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
            await dataService.CleanUpOnboardingAsync();
            logger.LogInformation("Cleaned up onboarding data");
        }
        catch (Exception e)
        {
            logger.LogError("Failed to clean up onboarding data: {error}", e.Message);
        }
    }
}