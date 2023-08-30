using Passwordless.AdminConsole.AuditLog;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.AuditLog.Loggers;
using Passwordless.AdminConsole.AuditLog.Storage;

namespace Passwordless.AdminConsole.Services;

public interface IAuditLogService
{
    Task LogOrganizationEvent(OrganizationEventDto organizationEvent);
    Task<OrganizationAuditLogResponse> GetAuditLogs(int organizationId, int pageNumber, int pageSize);
    Task<int> GetAuditLogCount(int organizationId);
    Task<ApplicationAuditLogResponse> GetAuditLogs(int pageNumber, int pageSize);
}

public class AuditLogService : IAuditLogService
{
    private readonly IScopedPasswordlessClient _scopedPasswordlessClient;
    private readonly IAuditLoggerProvider _provider;
    private readonly IAuditLoggerStorageProvider _storageProvider;

    public AuditLogService(IScopedPasswordlessClient scopedPasswordlessClient,
        IAuditLoggerProvider provider,
        IAuditLoggerStorageProvider storageProvider)
    {
        _scopedPasswordlessClient = scopedPasswordlessClient;
        _provider = provider;
        _storageProvider = storageProvider;
    }

    public async Task LogOrganizationEvent(OrganizationEventDto organizationEvent) =>
        await (await _provider.Create()).LogEvent(organizationEvent);

    public async Task<OrganizationAuditLogResponse> GetAuditLogs(int organizationId, int pageNumber, int pageSize) =>
        new(organizationId, (await _storageProvider.Create().GetOrganizationEvents(organizationId, pageNumber, pageSize))
            .Select(x => x.ToResponse()));

    public async Task<int> GetAuditLogCount(int organizationId) =>
        await _storageProvider.Create().GetOrganizationEventCount(organizationId);

    public async Task<ApplicationAuditLogResponse> GetAuditLogs(int pageNumber, int pageSize) =>
        await _scopedPasswordlessClient.GetApplicationAuditLog(pageNumber, pageSize);
}