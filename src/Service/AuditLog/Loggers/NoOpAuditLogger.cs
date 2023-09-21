using Passwordless.Common.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public class NoOpAuditLogger : IAuditLogger
{
    public void LogEvent(AuditEventDto auditEvent) { }

    public Task FlushAsync() => Task.CompletedTask;

    public static NoOpAuditLogger Instance { get; } = new();
}