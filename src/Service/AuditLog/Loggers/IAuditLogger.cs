using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public interface IAuditLogger
{
    void LogEvent(AuditEventDto auditEvent);
    void LogEvent(Func<IAuditLogContext, AuditEventDto> auditEventFunc);
    Task FlushAsync();
}