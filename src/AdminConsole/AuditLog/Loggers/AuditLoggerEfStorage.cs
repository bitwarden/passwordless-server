using AdminConsole.Db.AuditLog;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.AuditLog.Models;

namespace Passwordless.AdminConsole.AuditLog.Loggers;

public class AuditLoggerEfStorage : IAuditLoggerStorage, IAuditLogger
{
    private readonly ConsoleAuditLogDbContext _context;
    private readonly List<OrganizationAuditEvent> _items = new();
    public AuditLoggerEfStorage(ConsoleAuditLogDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<OrganizationEventDto>> GetOrganizationEvents(int organizationId, int pageNumber, int resultsPerPage) =>
        (await _context.OrganizationEvents
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.PerformedAt)
            .Skip(resultsPerPage * (pageNumber - 1))
            .Take(resultsPerPage)
            .AsNoTracking()
            .ToListAsync())
        .Select(x => x.ToDto());

    public async Task<int> GetOrganizationEventCount(int organizationId) => await _context.OrganizationEvents.CountAsync();
    public void LogEvent(OrganizationEventDto auditEvent)
    {
        _items.Add(auditEvent.ToNewEvent());
    }
    
    public async Task FlushAsync()
    {
        _context.OrganizationEvents.AddRange(_items);
        await _context.SaveChangesAsync();
    }
}