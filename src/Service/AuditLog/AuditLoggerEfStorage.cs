using Passwordless.Service.Models;
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
}