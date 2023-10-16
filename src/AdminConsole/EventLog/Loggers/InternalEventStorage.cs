using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Configuration;
using Passwordless.AdminConsole.Db;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public interface IInternalEventStorage
{
    Task DeleteExpiredEvents(CancellationToken cancellationToken);
}

public class InternalEventStorage<TDbContext> : IInternalEventStorage where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IOptions<PlansOptions> _planOptionsConfig;
    private readonly ISystemClock _systemClock;

    public InternalEventStorage(IDbContextFactory<TDbContext> dbContextFactory,
        IOptions<PlansOptions> planOptionsConfig,
        ISystemClock systemClock)
    {
        _dbContextFactory = dbContextFactory;
        _planOptionsConfig = planOptionsConfig;
        _systemClock = systemClock;
    }

    public async Task DeleteExpiredEvents(CancellationToken cancellationToken)
    {
        _planOptionsConfig.Value.TryGetValue("Enterprise", out var enterprisePlan);

        var retentionDays = enterprisePlan?.EventLoggingRetentionPeriod ?? 7;

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var events = await db.OrganizationEvents
            .Where(x => x.PerformedAt <= _systemClock.UtcNow.UtcDateTime.AddDays(-retentionDays))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        db.RemoveRange(events);
        await db.SaveChangesAsync(cancellationToken);
    }
}