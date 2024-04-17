using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Background;

namespace Passwordless.AdminConsole.BackgroundServices;

public sealed class ApplicationCleanupBackgroundService : BasePeriodicBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationCleanupBackgroundService> _logger;

    public ApplicationCleanupBackgroundService(IServiceProvider serviceProvider, TimeProvider timeProvider, ILogger<ApplicationCleanupBackgroundService> logger)
        : base(new TimeOnly(0), TimeSpan.FromDays(1), timeProvider, logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IPasswordlessManagementClient>();
            var applicationService = scope.ServiceProvider.GetRequiredService<IApplicationService>();
            var applicationIds = await client.ListApplicationsPendingDeletionAsync();
            foreach (var applicationId in applicationIds)
            {
                if (await client.DeleteApplicationAsync(applicationId))
                {
                    await applicationService.DeleteAsync(applicationId);
                }
                else
                {
                    _logger.LogError("Failed to delete application: {appId}", applicationId);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to delete applications: {error}", e.Message);
        }
    }
}