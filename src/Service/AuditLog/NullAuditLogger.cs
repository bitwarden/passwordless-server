using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog;

public interface INullAuditLogger : IAuditLogger { }

public class NullAuditLogger : INullAuditLogger
{
    public Task LogEvent(AuditEventDto auditEvent) => Task.CompletedTask;
}