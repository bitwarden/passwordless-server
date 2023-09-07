using Microsoft.EntityFrameworkCore;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Storage.Ef.AuditLog;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerEfStorage : IAuditLogStorage, IAuditLogger
{
    private readonly DbAuditLogContext _db;
    private readonly List<ApplicationAuditEvent> _items = new();

    public AuditLoggerEfStorage(DbAuditLogContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ApplicationAuditEvent>> GetAuditLogAsync(string tenantId, int pageNumber, int resultsPerPage, CancellationToken cancellationToken) =>
        await _db.AppEvents
            .OrderByDescending(x => x.PerformedAt)
            .Skip(resultsPerPage * (pageNumber - 1))
            .Take(resultsPerPage)
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

    public async Task<int> GetAuditLogCountAsync(string tenantId, CancellationToken cancellationToken) =>
        await _db.AppEvents.CountAsync(x => x.TenantId == tenantId, cancellationToken);

    public void LogEvent(AuditEventDto auditEvent)
    {
        _items.Add(auditEvent.ToEvent());
    }

    public async Task FlushAsync()
    {
        _db.AppEvents.AddRange(_items);
        await _db.SaveChangesAsync();
    }
}