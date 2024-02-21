using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Models;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class EventLoggerEfWriteStorage : IEventLogger
{
    private readonly List<OrganizationEvent> _events = new();
    private readonly ConsoleDbContext _db;

    public EventLoggerEfWriteStorage(ConsoleDbContext db)
    {
        _db = db;
    }

    public virtual void LogEvent(OrganizationEventDto @event)
    {
        _events.Add(@event.ToNewEvent());
    }

    public async Task FlushAsync()
    {
        if (_events.Any())
        {
            _db.OrganizationEvents.AddRange(_events);
            await _db.SaveChangesAsync();
        }
    }
}