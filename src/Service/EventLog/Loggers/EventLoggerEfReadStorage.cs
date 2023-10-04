using Microsoft.EntityFrameworkCore;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.EventLog.Loggers;

public class EventLoggerEfReadStorage : IEventLogStorage
{
    private readonly DbTenantContext _db;

    public EventLoggerEfReadStorage(DbTenantContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ApplicationEvent>> GetEventLogAsync(int pageNumber, int resultsPerPage, CancellationToken cancellationToken) =>
        await _db.ApplicationEvents
            .OrderByDescending(x => x.PerformedAt)
            .Skip(resultsPerPage * (pageNumber - 1))
            .Take(resultsPerPage)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<int> GetEventLogCountAsync(CancellationToken cancellationToken) =>
        await _db.ApplicationEvents.CountAsync(cancellationToken);
}