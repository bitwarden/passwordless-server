using Passwordless.AdminConsole.EventLog.DTOs;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public interface IEventLoggerStorage
{
    Task<IEnumerable<OrganizationEventDto>> GetOrganizationEvents(int organizationId, int pageNumber, int resultsPerPage);
    Task<int> GetOrganizationEventCount(int organizationId);
}