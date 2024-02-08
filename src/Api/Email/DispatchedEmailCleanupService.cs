using Passwordless.Common.Background;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Api.Email;

public class DispatchedEmailCleanupService(TimeProvider timeProvider, ITenantStorage tenantStorage)
    : BasePeriodicBackgroundService(new TimeOnly(00, 00, 00), TimeSpan.FromDays(1), timeProvider)
{
    protected override async Task DoWorkAsync(CancellationToken cancellationToken) =>
        // We only need 30 days worth of emails, but let's keep a small extra buffer just in case
        await tenantStorage.DeleteOldDispatchedEmailsAsync(TimeSpan.FromDays(50));
}