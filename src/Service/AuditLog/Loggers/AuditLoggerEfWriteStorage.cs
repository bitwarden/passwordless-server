using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerEfWriteStorage : IAuditLogger
{
    private readonly DbGlobalContext _storage;

    public AuditLoggerEfWriteStorage(DbGlobalContext storage)
    {
        _storage = storage;
    }

    private readonly List<ApplicationAuditEvent> _items = new();
    public void LogEvent(AuditEventDto auditEvent)
    {
        _items.Add(auditEvent.ToEvent());
    }
    public async Task FlushAsync()
    {
        _storage.ApplicationEvents.AddRange(_items);
        await _storage.SaveChangesAsync();
    }
}