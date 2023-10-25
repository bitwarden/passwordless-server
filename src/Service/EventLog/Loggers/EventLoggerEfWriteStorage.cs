using Passwordless.Service.EventLog.Mappings;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.EventLog.Loggers;

public class EventLoggerEfWriteStorage : IEventLogger
{
    private readonly DbGlobalContext _storage;
    private readonly IEventLogContext _eventLogContext;
    private readonly List<ApplicationEvent> _items = new();

    public EventLoggerEfWriteStorage(DbGlobalContext storage, IEventLogContext eventLogContext)
    {
        _storage = storage;
        _eventLogContext = eventLogContext;
    }

    public void LogEvent(EventDto @event)
    {
        _items.Add(@event.ToEvent());
    }

    public void LogEvent(Func<IEventLogContext, EventDto> eventFunc)
    {
        LogEvent(eventFunc(_eventLogContext));
    }

    public async Task FlushAsync()
    {
        _storage.ApplicationEvents.AddRange(_items);
        await _storage.SaveChangesAsync();
    }
}