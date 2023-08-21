using Passwordless.Service.Features;

namespace Passwordless.Service.AuditLog;

public interface IAuditLoggerFactory
{
    Task<IAuditLogger> Create();
}

public class AuditLoggerFactory : IAuditLoggerFactory
{
    private readonly IFeatureContextProvider _provider;
    private readonly INullAuditLogger _nullAuditLogger;
    private readonly IAuditLogger _auditLogger;

    public AuditLoggerFactory(IFeatureContextProvider provider, INullAuditLogger nullAuditLogger, IAuditLogger auditLogger)
    {
        _provider = provider;
        _nullAuditLogger = nullAuditLogger;
        _auditLogger = auditLogger;
    }

    public async Task<IAuditLogger> Create() =>
        (await _provider.UseContext()).AuditLoggingIsEnabled
            ? _auditLogger
            : _nullAuditLogger;
}