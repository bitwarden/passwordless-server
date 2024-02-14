using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Billing.Configuration;
using Passwordless.AdminConsole.Db;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public interface IInternalEventLogStorageContext
{
    Task DeleteExpiredEvents(CancellationToken cancellationToken);
}

public class InternalEventLogStorageContext : IInternalEventLogStorageContext
{
    private readonly ConsoleDbContext _db;
    private readonly BillingOptions _planOptionsConfig;
    private readonly TimeProvider _timeProvider;

    public InternalEventLogStorageContext(ConsoleDbContext db,
        IOptions<BillingOptions> planOptionsConfig,
        TimeProvider timeProvider)
    {
        _db = db;
        _planOptionsConfig = planOptionsConfig.Value;
        _timeProvider = timeProvider;
    }

    public async Task DeleteExpiredEvents(CancellationToken cancellationToken)
    {
        var organizations = await _db.OrganizationEvents
            .Select(x => x.OrganizationId)
            .Distinct()
            .Select(organization => new
            {
                OrganizationId = organization,
                BillingPlan = _db.Applications
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
            await _db.OrganizationEvents
                .Where(x => x.OrganizationId == organization.OrganizationId && x.PerformedAt < now.AddDays(-features.EventLoggingRetentionPeriod))
                .ExecuteDeleteAsync();
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}