using AdminConsole.Db.AuditLog;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.AuditLog.DTOs;

namespace Passwordless.AdminConsole.AuditLog.Storage;

public class AuditLoggerEfStorage : IAuditLoggerEfStorage
{
    private readonly ConsoleAuditLogDbContext _context;

    public AuditLoggerEfStorage(ConsoleAuditLogDbContext context)
    {
        _context = context;
    }

    public async Task WriteEvent(OrganizationEventDto auditEvent)
    {
        await _context.OrganizationEvents.AddAsync(auditEvent.ToNewEvent());
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrganizationEventDto>> GetOrganizationEvents(int organizationId) =>
        (await _context.OrganizationEvents
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync())
        .Select(x => x.ToDto());
}