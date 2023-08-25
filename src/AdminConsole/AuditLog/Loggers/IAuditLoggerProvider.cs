namespace Passwordless.AdminConsole.AuditLog.Loggers;

public interface IAuditLoggerProvider
{
    Task<IAuditLogger> Create();
}