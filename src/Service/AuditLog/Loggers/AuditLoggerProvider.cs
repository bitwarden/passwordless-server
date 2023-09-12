using Passwordless.Service.Features;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerProvider
{
    private readonly IFeatureContextProvider _featureContextProvider;
    private readonly IAuditLogger _auditLogger;

    public AuditLoggerProvider(IFeatureContextProvider featureContextProvider, IAuditLogger auditLogger)
    {
        _featureContextProvider = featureContextProvider;
        _auditLogger = auditLogger;
    }

    public async Task<IAuditLogger> Create() =>
        (await _featureContextProvider.UseContext()).AuditLoggingIsEnabled
            ? _auditLogger
            : NoOpAuditLogger.Instance;
}