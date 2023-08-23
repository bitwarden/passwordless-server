using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog;

public interface IAuditLogStorage
{
    Task WriteEventAsync(AuditEventDto auditEvent);
    Task<IEnumerable<AuditEvent>> GetAuditLogAsync(int organizationId, CancellationToken cancellationToken);
}