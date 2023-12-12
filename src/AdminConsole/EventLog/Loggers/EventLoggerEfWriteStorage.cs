using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Models;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class EventLoggerEfWriteStorage<TDbContext> : IEventLogger where TDbContext : ConsoleDbContext
{
    private readonly List<OrganizationEvent> _events = new();
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    public EventLoggerEfWriteStorage(IDbContextFactory<TDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public virtual void LogEvent(OrganizationEventDto @event)
    {
        _events.Add(@event.ToNewEvent());
    }

    public async Task FlushAsync()
    {
        if (_events.Any())
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.OrganizationEvents.AddRange(_events);
            await db.SaveChangesAsync();
        }
    }
}