using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog;

public interface IAuditLogStorage
{
    Task WriteEventAsync(AuditEventDto auditEvent);
    Task<IEnumerable<AuditEvent>> GetAuditLogAsync(string tenantId, CancellationToken cancellationToken);
}