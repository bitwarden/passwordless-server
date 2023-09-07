using Passwordless.Common.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public interface IAuditLogger
{
    void LogEvent(AuditEventDto auditEvent);
    Task FlushAsync();
}