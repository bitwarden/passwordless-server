using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Billing.Constants;
using Passwordless.AdminConsole.Db;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public interface IInternalEventLogStorageContext
{
    Task DeleteExpiredEvents(CancellationToken cancellationToken);
}

public class InternalEventLogStorageContext<TDbContext> : IInternalEventLogStorageContext where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly StripeOptions _planOptionsConfig;
    private readonly ISystemClock _systemClock;

    public InternalEventLogStorageContext(IDbContextFactory<TDbContext> dbContextFactory,
        IOptions<StripeOptions> planOptionsConfig,
        ISystemClock systemClock)
    {
        _dbContextFactory = dbContextFactory;
        _planOptionsConfig = planOptionsConfig.Value;
        _systemClock = systemClock;
    }

    public async Task DeleteExpiredEvents(CancellationToken cancellationToken)
    {
        var plan = _planOptionsConfig.Plans[PlanConstants.Enterprise];
        var retentionDays = plan.Features.EventLoggingRetentionPeriod;

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var events = await db.OrganizationEvents
            .Where(x => x.PerformedAt <= _systemClock.UtcNow.UtcDateTime.AddDays(-retentionDays))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        db.RemoveRange(events);
        await db.SaveChangesAsync(cancellationToken);
    }
}