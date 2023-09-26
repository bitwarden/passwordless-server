using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public class NoOpAuditLogger : IAuditLogger
{
    public void LogEvent(AuditEventDto auditEvent) { }

    public void LogEvent(Func<IAuditLogContext, AuditEventDto> auditEventFunc) { }

    public Task FlushAsync() => Task.CompletedTask;

    public static NoOpAuditLogger Instance { get; } = new();
}