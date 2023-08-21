using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog;

public interface IAuditLogStorage
{
    Task WriteEventAsync(AuditEventDto auditEvent);
}