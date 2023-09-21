using AdminConsole.Db;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class AuditLoggerEfUnauthenticatedWriteStorage : AbstractAuditLoggerEfWriteStorage
{
    private readonly OrganizationFeatureService _organizationFeatureService;

    public AuditLoggerEfUnauthenticatedWriteStorage(ConsoleDbContext context, OrganizationFeatureService organizationFeatureService) : base(context)
    {
        _organizationFeatureService = organizationFeatureService;
    }

    public override void LogEvent(OrganizationEventDto auditEvent)
    {
        if (HasAuditLoggingEnabled(auditEvent.OrganizationId))
        {
            base.LogEvent(auditEvent);
        }
    }

    private bool HasAuditLoggingEnabled(int organizationId)
    {
        return _organizationFeatureService.GetOrganizationFeatures(organizationId).AuditLoggingIsEnabled;
    }
}