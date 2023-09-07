using Microsoft.EntityFrameworkCore;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerEfStorage : IAuditLogStorage, IAuditLogger
{
    private readonly DbTenantContext _db;
    private readonly List<ApplicationAuditEvent> _items = new();

    public AuditLoggerEfStorage(DbTenantContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ApplicationAuditEvent>> GetAuditLogAsync(string tenantId, int pageNumber, int resultsPerPage, CancellationToken cancellationToken) =>
        await _db.ApplicationEvents
            .OrderByDescending(x => x.PerformedAt)
            .Skip(resultsPerPage * (pageNumber - 1))
            .Take(resultsPerPage)
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

    public async Task<int> GetAuditLogCountAsync(string tenantId, CancellationToken cancellationToken) =>
        await _db.ApplicationEvents.CountAsync(x => x.TenantId == tenantId, cancellationToken);

    public void LogEvent(AuditEventDto auditEvent)
    {
        _items.Add(auditEvent.ToEvent());
    }

    public async Task FlushAsync()
    {
        _db.ApplicationEvents.AddRange(_items);
        await _db.SaveChangesAsync();
    }
}