using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.Services;

public interface IAuditLogService
{
    Task LogOrganizationEvent(AuditLogEventRequest organizationEvent);
    Task<OrganizationAuditLogResponse> GetAuditLogs(int organizationId);
}

public class AuditLogService : IAuditLogService
{
    private readonly IPasswordlessManagementClient _client;

    public AuditLogService(IPasswordlessManagementClient client)
    {
        _client = client;
    }

    public async Task LogOrganizationEvent(AuditLogEventRequest organizationEvent)
    {
        await _client.LogEventAsync(organizationEvent);
    }

    public async Task<OrganizationAuditLogResponse> GetAuditLogs(int organizationId)
    {
        return await _client.GetOrganizationAuditLog(organizationId);
    }
}