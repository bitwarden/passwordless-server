namespace Passwordless.AdminConsole.AuditLog.Storage;

public interface IAuditLoggerStorageProvider
{
    IAuditLoggerStorage Create();
}