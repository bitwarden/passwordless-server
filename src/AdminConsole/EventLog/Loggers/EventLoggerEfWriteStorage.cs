using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Models;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class EventLoggerEfWriteStorage : IEventLogger
{
    private readonly ConsoleDbContext _context;
    private readonly List<OrganizationEvent> _events = new();

    public EventLoggerEfWriteStorage(ConsoleDbContext context)
    {
        _context = context;
    }

    public virtual void LogEvent(OrganizationEventDto @event)
    {
        _events.Add(@event.ToNewEvent());
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