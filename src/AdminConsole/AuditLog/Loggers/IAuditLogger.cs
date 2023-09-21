using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public interface IAuditLogger
{
    void LogEvent(OrganizationEventDto auditEvent);
    Task FlushAsync();
}