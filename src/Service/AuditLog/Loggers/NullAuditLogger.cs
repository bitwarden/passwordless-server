using Passwordless.Common.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public interface INullAuditLogger : IAuditLogger { }

public class NullAuditLogger : INullAuditLogger
{
    public Task LogEvent(AuditEventDto auditEvent) => Task.CompletedTask;
}