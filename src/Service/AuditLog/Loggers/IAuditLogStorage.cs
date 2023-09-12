using Passwordless.Service.AuditLog.Models;

namespace Passwordless.Service.AuditLog.Loggers;

public interface IAuditLogStorage
{
    Task<IEnumerable<ApplicationAuditEvent>> GetAuditLogAsync(int pageNumber, int resultsPerPage, CancellationToken cancellationToken);
    Task<int> GetAuditLogCountAsync(CancellationToken cancellationToken);
}