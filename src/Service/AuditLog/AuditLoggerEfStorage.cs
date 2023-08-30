using Microsoft.EntityFrameworkCore;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Storage.Ef.AuditLog;

namespace Passwordless.Service.AuditLog;

public class AuditLoggerEfStorage : IAuditLogStorage
{
    private readonly DbAuditLogContext _db;

    public AuditLoggerEfStorage(DbAuditLogContext db)
    {
        _db = db;
    }

    public async Task WriteEventAsync(AuditEventDto auditEvent)
    {
        await _db.AppEvents.AddAsync(auditEvent.ToEvent());
        await _db.SaveChangesAsync();
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
}