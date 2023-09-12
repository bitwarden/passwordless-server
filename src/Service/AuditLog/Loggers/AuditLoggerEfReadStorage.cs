using Microsoft.EntityFrameworkCore;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerEfReadStorage : IAuditLogStorage
{
    private readonly DbTenantContext _db;

    public AuditLoggerEfReadStorage(DbTenantContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ApplicationAuditEvent>> GetAuditLogAsync(int pageNumber, int resultsPerPage, CancellationToken cancellationToken) =>
        await _db.ApplicationEvents
            .OrderByDescending(x => x.PerformedAt)
            .Skip(resultsPerPage * (pageNumber - 1))
            .Take(resultsPerPage)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<int> GetAuditLogCountAsync(CancellationToken cancellationToken) =>
        await _db.ApplicationEvents.CountAsync(cancellationToken);
}