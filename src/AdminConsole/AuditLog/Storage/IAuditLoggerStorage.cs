using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Storage;

public interface IAuditLoggerStorage
{
    Task WriteEvent(OrganizationEventDto auditEvent);
    Task<IEnumerable<OrganizationEventDto>> GetOrganizationEvents(int organizationId);
}