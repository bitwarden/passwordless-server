using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.DTOs;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class EventLoggerEfReadStorage : IEventLoggerStorage
{
    private readonly ConsoleDbContext _context;

    public EventLoggerEfReadStorage(ConsoleDbContext context)
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

    public async Task<int> GetOrganizationEventCount(int organizationId) =>
        await _context.OrganizationEvents
            .CountAsync(x => x.OrganizationId == organizationId);
}