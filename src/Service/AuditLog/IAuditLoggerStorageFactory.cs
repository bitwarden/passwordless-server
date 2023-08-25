using Passwordless.Common.AuditLog;

namespace Passwordless.Service.AuditLog;

public interface IAuditLoggerStorageFactory
{
    IAuditLogStorage Create();
}