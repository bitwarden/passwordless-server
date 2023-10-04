using Passwordless.AdminConsole.EventLog;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Loggers;

namespace Passwordless.AdminConsole.Services;

public interface IEventLogService
{
    Task<OrganizationEventLogResponse> GetEventLogs(int organizationId, int pageNumber, int pageSize);
    Task<int> GetEventLogCount(int organizationId);
    Task<ApplicationEventLogResponse> GetEventLogs(int pageNumber, int pageSize);
}

public class EventLogService : IEventLogService
{
    private readonly IScopedPasswordlessClient _scopedPasswordlessClient;
    private readonly IEventLoggerStorage _storage;

    public EventLogService(IScopedPasswordlessClient scopedPasswordlessClient,
        IEventLoggerStorage storage)
    {
        _scopedPasswordlessClient = scopedPasswordlessClient;
        _storage = storage;
    }

    public async Task<OrganizationEventLogResponse> GetEventLogs(int organizationId, int pageNumber, int pageSize) =>
        new(organizationId, (await _storage.GetOrganizationEvents(organizationId, pageNumber, pageSize))
            .Select(x => x.ToResponse()));

    public async Task<int> GetEventLogCount(int organizationId) =>
        await _storage.GetOrganizationEventCount(organizationId);

    public async Task<ApplicationEventLogResponse> GetEventLogs(int pageNumber, int pageSize) =>
        await _scopedPasswordlessClient.GetApplicationEventLog(pageNumber, pageSize);
}