using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public interface IAuditLoggerStorage
{
    Task<IEnumerable<OrganizationEventDto>> GetOrganizationEvents(int organizationId, int pageNumber, int resultsPerPage);
    Task<int> GetOrganizationEventCount(int organizationId);
}