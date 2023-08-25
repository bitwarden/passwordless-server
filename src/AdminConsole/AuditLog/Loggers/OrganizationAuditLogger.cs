using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.AuditLog.Storage;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class OrganizationAuditLogger : IOrganizationAuditLogger
{
    private readonly IAuditLoggerStorageProvider _provider;

    public OrganizationAuditLogger(IAuditLoggerStorageProvider provider)
    {
        _provider = provider;
    }

    public Task LogEvent(OrganizationEventDto auditEvent) =>
        _provider.Create().WriteEvent(auditEvent);
}