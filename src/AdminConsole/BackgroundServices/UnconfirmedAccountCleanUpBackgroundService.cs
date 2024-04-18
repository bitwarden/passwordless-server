using Passwordless.AdminConsole.Services;
using Passwordless.Common.Background;

namespace Passwordless.AdminConsole.BackgroundServices;

public class UnconfirmedAccountCleanUpBackgroundService(
    ILogger<UnconfirmedAccountCleanUpBackgroundService> logger,
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
            var results = await dataService.CleanUpUnconfirmedAccounts(cancellationToken);
            logger.LogInformation("Cleaned up {orgCount} accounts with only unconfirmed users. Total Users Deleted: {userCount}", results.DeletedOrganizations, results.DeletedUsers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clean up unconfirmed accounts.");
        }
    }
}

public record UnconfirmedAccountCleanUpQueryResult(int DeletedOrganizations, int DeletedUsers);