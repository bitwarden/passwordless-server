using AdminConsole.Services;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class AuditLoggerProvider : IAuditLoggerProvider
{
    private readonly IOrganizationAuditLogger _organizationAuditLogger;
    private readonly INoOpAuditLogger _noOpAuditLogger;
    private readonly DataService _dataService;

    public AuditLoggerProvider(IOrganizationAuditLogger organizationAuditLogger, INoOpAuditLogger noOpAuditLogger, DataService dataService)
    {
        _organizationAuditLogger = organizationAuditLogger;
        _noOpAuditLogger = noOpAuditLogger;
        _dataService = dataService;
    }

    public async Task<IAuditLogger> Create() =>
        true
            ? _organizationAuditLogger
            : _noOpAuditLogger;
}