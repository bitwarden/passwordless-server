using Passwordless.Service.Models;

namespace Passwordless.Service.AuditLog;

public interface IDeveloperLogger : IAuditLogger { }

public class DeveloperLogger : IDeveloperLogger
{
    public Task LogEvent(AuditEventDto auditEvent)
    {
        throw new NotImplementedException();
    }
}