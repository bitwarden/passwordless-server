using Passwordless.Common.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public interface IAuditLogger
{
    Task LogEvent(AuditEventDto auditEvent);
}

public class AuditLogger : IAuditLogger
{
    private readonly IAuditLoggerStorageFactory _factory;

    public AuditLogger(IAuditLoggerStorageFactory factory)
    {
        _factory = factory;
    }

    public Task LogEvent(AuditEventDto auditEvent)
    {
        return _factory.Create().WriteEventAsync(auditEvent);
    }
}