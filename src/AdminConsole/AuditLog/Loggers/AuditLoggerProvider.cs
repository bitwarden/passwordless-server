using Passwordless.AdminConsole.Features;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class AuditLoggerProvider : IAuditLoggerProvider
{
    private readonly IOrganizationAuditLogger _organizationAuditLogger;
    private readonly INoOpAuditLogger _noOpAuditLogger;
    private readonly IFeaturesContext _featuresContext;

    public AuditLoggerProvider(IOrganizationAuditLogger organizationAuditLogger, INoOpAuditLogger noOpAuditLogger, IFeaturesContext featuresContext)
    {
        _organizationAuditLogger = organizationAuditLogger;
        _noOpAuditLogger = noOpAuditLogger;
        _featuresContext = featuresContext;
    }

    public async Task<IAuditLogger> Create() =>
        await _featuresContext.IsAuditLoggingEnabled()
            ? _organizationAuditLogger
            : _noOpAuditLogger;
}