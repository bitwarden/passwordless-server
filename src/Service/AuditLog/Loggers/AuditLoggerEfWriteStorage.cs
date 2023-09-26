using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerEfWriteStorage : IAuditLogger
{
    private readonly DbGlobalContext _storage;
    private readonly IAuditLogContext _auditLogContext;
    private readonly List<ApplicationAuditEvent> _items = new();

    public AuditLoggerEfWriteStorage(DbGlobalContext storage, IAuditLogContext auditLogContext)
    {
        _storage = storage;
        _auditLogContext = auditLogContext;
    }

    public void LogEvent(AuditEventDto auditEvent)
    {
        _items.Add(auditEvent.ToEvent());
    }

    public void LogEvent(Func<IAuditLogContext, AuditEventDto> auditEventFunc)
    {
        LogEvent(auditEventFunc(_auditLogContext));
    }

    public async Task FlushAsync()
    {
        _storage.ApplicationEvents.AddRange(_items);
        await _storage.SaveChangesAsync();
    }
}