using Passwordless.AdminConsole.Models.DTOs;

namespace Passwordless.AdminConsole.Services;

public interface IAuditLogService
{
    Task LogOrganizationEvent(AuditLogEventRequest organizationEvent);
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
}