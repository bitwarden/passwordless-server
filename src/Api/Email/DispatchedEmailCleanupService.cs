using Passwordless.Common.Background;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Email;

public class DispatchedEmailCleanupService(
    TimeProvider timeProvider,
    ILogger<DispatchedEmailCleanupService> logger,
    IServiceProvider serviceProvider)
    : BasePeriodicBackgroundService(
        new TimeOnly(00, 00, 00),
        TimeSpan.FromDays(1),
        timeProvider, logger)
{
    protected override async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        var scope = serviceProvider.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IGlobalStorage>();

        // We only need 30 days worth of emails, but let's keep a small extra buffer just in case
        await storage.DeleteOldDispatchedEmailsAsync(TimeSpan.FromDays(50));

    }
}