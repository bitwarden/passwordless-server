namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class AuditLoggerProvider : IAuditLoggerProvider
{
    private readonly IOrganizationAuditLogger _organizationAuditLogger;
    private readonly INoOpAuditLogger _noOpAuditLogger;
    private readonly ICurrentContext _currentContext;

    public AuditLoggerProvider(IOrganizationAuditLogger organizationAuditLogger, INoOpAuditLogger noOpAuditLogger, ICurrentContext currentContext)
    {
        _organizationAuditLogger = organizationAuditLogger;
        _noOpAuditLogger = noOpAuditLogger;
        _currentContext = currentContext;
    }

    public Task<IAuditLogger> Create() =>
        _currentContext.OrganizationFeatures.AuditLoggingIsEnabled
            ? Task.FromResult<IAuditLogger>(_organizationAuditLogger)
            : Task.FromResult<IAuditLogger>(_noOpAuditLogger);
}