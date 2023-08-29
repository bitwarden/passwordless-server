using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Storage;

public interface IAuditLoggerStorage
{
    Task WriteEvent(OrganizationEventDto auditEvent);
    Task<IEnumerable<OrganizationEventDto>> GetOrganizationEvents(int organizationId, int pageNumber, int resultsPerPage);
    Task<int> GetOrganizationEventCount(int organizationId);
}