using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Helpers;

namespace Passwordless.AdminConsole.Services;

public interface IEventLogService
{
    Task<OrganizationEventLogResponse> GetOrganizationEventLogsAsync(int pageNumber, int pageSize);
    Task<int> GetOrganizationEventLogCountAsync();
    Task<ApplicationEventLogResponse> GetEventLogs(int pageNumber, int pageSize);
}

public class EventLogService : IEventLogService
{
    private readonly IScopedPasswordlessClient _scopedPasswordlessClient;
    private readonly IEventLoggerStorage _storage;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EventLogService(
        IScopedPasswordlessClient scopedPasswordlessClient,
        IEventLoggerStorage storage,
        IHttpContextAccessor httpContextAccessor)
    {
        _scopedPasswordlessClient = scopedPasswordlessClient;
        _storage = storage;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OrganizationEventLogResponse> GetOrganizationEventLogsAsync(int pageNumber, int pageSize)
    {
        var organizationId = _httpContextAccessor.HttpContext?.User.GetOrgId();

        if (!organizationId.HasValue)
        {
            throw new InvalidOperationException("OrganizationId is not available in the current context.");
        }

        var events = (await _storage.GetOrganizationEvents(organizationId.Value, pageNumber, pageSize))
            .Select(x => x.ToResponse());

        return new OrganizationEventLogResponse(events);
    }



    public async Task<int> GetOrganizationEventLogCountAsync()
    {
        var organizationId = _httpContextAccessor.HttpContext?.User.GetOrgId();

        if (!organizationId.HasValue)
        {
            throw new InvalidOperationException("OrganizationId is not available in the current context.");
        }

        return await _storage.GetOrganizationEventCount(organizationId.Value);
    }

    public async Task<ApplicationEventLogResponse> GetEventLogs(int pageNumber, int pageSize) =>
        await _scopedPasswordlessClient.GetApplicationEventLog(pageNumber, pageSize);
}