using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.DTOs;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class EventLoggerEfReadStorage<TDbContext> : IEventLoggerStorage where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    public EventLoggerEfReadStorage(IDbContextFactory<TDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<OrganizationEventDto>> GetOrganizationEvents(int organizationId, int pageNumber,
        int resultsPerPage)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return (await db.OrganizationEvents
                .Where(x => x.OrganizationId == organizationId)
                .OrderByDescending(x => x.PerformedAt)
                .Skip(resultsPerPage * (pageNumber - 1))
                .Take(resultsPerPage)
                .AsNoTracking()
                .ToListAsync())
            .Select(x => x.ToDto());
    }



    public async Task<int> GetOrganizationEventCount(int organizationId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.OrganizationEvents
            .CountAsync(x => x.OrganizationId == organizationId);
    }
}