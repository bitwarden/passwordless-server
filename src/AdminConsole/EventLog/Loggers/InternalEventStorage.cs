using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
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
    private readonly TimeProvider _timeProvider;

    public InternalEventLogStorageContext(IDbContextFactory<TDbContext> dbContextFactory,
        IOptions<StripeOptions> planOptionsConfig,
        TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _planOptionsConfig = planOptionsConfig.Value;
        _timeProvider = timeProvider;
    }

    public async Task DeleteExpiredEvents(CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var organizations = await db.OrganizationEvents
            .Select(x => x.OrganizationId)
            .Distinct()
            .Select(organization => new
            {
                OrganizationId = organization,
                BillingPlan = db.Applications
                    .Where(app => app.OrganizationId == organization)
                    .GroupBy(app => app.BillingPlan)
                    .Select(group => group.Key)
                    .FirstOrDefault() ?? _planOptionsConfig.Store.Free
            })
            .ToListAsync(cancellationToken);

        foreach (var organization in organizations)
        {
            var features = _planOptionsConfig.Plans[organization.BillingPlan].Features;
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            await db.OrganizationEvents
                .Where(x => x.OrganizationId == organization.OrganizationId && x.PerformedAt < now.AddDays(-features.EventLoggingRetentionPeriod))
                .ExecuteDeleteAsync();
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}