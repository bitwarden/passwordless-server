using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerProvider
{
    private readonly IAuditLogContext _auditLogContext;
    private readonly IAuditLogger _auditLogger;

    public AuditLoggerProvider(IAuditLogContext auditLogContext, IAuditLogger auditLogger)
    {
        _auditLogContext = auditLogContext;
        _auditLogger = auditLogger;
    }

    public async Task<IAuditLogger> Create() =>
        _auditLogContext.Features.AuditLoggingIsEnabled
            ? _auditLogger
            : NoOpAuditLogger.Instance;
}