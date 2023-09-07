using Passwordless.AdminConsole.AuditLog;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.AuditLog.Loggers;

namespace Passwordless.AdminConsole.Services;

public interface IAuditLogService
{
    Task<OrganizationAuditLogResponse> GetAuditLogs(int organizationId, int pageNumber, int pageSize);
    Task<int> GetAuditLogCount(int organizationId);
    Task<ApplicationAuditLogResponse> GetAuditLogs(int pageNumber, int pageSize);
}

public class AuditLogService : IAuditLogService
{
    private readonly IScopedPasswordlessClient _scopedPasswordlessClient;
    private readonly IAuditLoggerStorage _storage;

    public AuditLogService(IScopedPasswordlessClient scopedPasswordlessClient,
        IAuditLoggerStorage storage)
    {
        _scopedPasswordlessClient = scopedPasswordlessClient;
        _storage = storage;
    }

    public async Task<OrganizationAuditLogResponse> GetAuditLogs(int organizationId, int pageNumber, int pageSize) =>
        new(organizationId, (await _storage.GetOrganizationEvents(organizationId, pageNumber, pageSize))
            .Select(x => x.ToResponse()));

    public async Task<int> GetAuditLogCount(int organizationId) =>
        await _storage.GetOrganizationEventCount(organizationId);

    public async Task<ApplicationAuditLogResponse> GetAuditLogs(int pageNumber, int pageSize) =>
        await _scopedPasswordlessClient.GetApplicationAuditLog(pageNumber, pageSize);
}