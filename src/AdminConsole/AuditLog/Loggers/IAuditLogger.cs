using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public interface IAuditLogger
{
    Task LogEvent(OrganizationEventDto auditEvent);
}