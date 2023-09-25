using AdminConsole.Db;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.AuditLog.Models;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class AuditLoggerEfWriteStorage : IAuditLogger
{
    private readonly ConsoleDbContext _context;
    private readonly List<OrganizationAuditEvent> _events = new();

    public AuditLoggerEfWriteStorage(ConsoleDbContext context)
    {
        _context = context;
    }

    public virtual void LogEvent(OrganizationEventDto auditEvent)
    {
        _events.Add(auditEvent.ToNewEvent());
    }

    public async Task FlushAsync()
    {
        if (_events.Any())
        {
            _context.OrganizationEvents.AddRange(_events);
            await _context.SaveChangesAsync();
        }
    }
}