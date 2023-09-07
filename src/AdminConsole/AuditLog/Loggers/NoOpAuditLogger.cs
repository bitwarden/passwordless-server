using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class NoOpAuditLogger : IAuditLogger
{
    public void LogEvent(OrganizationEventDto auditEvent)
    {
    }

    public Task FlushAsync() => Task.CompletedTask;

    public static NoOpAuditLogger Instance { get; } = new();
}