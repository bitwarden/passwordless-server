using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class NoOpAuditLogger : INoOpAuditLogger
{
    public Task LogEvent(OrganizationEventDto auditEvent) => Task.CompletedTask;
}