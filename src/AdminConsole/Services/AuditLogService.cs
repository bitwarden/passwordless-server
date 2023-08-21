using Passwordless.AdminConsole.Models.DTOs;

namespace Passwordless.AdminConsole.Services;

public interface IAuditLogService
{
    Task LogOrganizationEvent(AuditLogEventRequest createOrganizationCreatedEvent);
    Task LogApplicationEvent();
}

public class AuditLogService : IAuditLogService
{
    private readonly IPasswordlessManagementClient _client;

    public AuditLogService(IPasswordlessManagementClient client)
    {
        _client = client;
    }
    
    public async Task LogOrganizationEvent(AuditLogEventRequest createOrganizationCreatedEvent)
    {
        await _client.LogEventAsync(createOrganizationCreatedEvent);
    }

    public Task LogApplicationEvent()
    {
        throw new NotImplementedException();
    }
}