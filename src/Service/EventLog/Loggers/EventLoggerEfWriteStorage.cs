using Passwordless.Service.EventLog.Mappings;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.EventLog.Loggers;

public class EventLoggerEfWriteStorage : IEventLogger
{
    private readonly DbGlobalContext _storage;
    private readonly IEventLogContext _eventLogContext;
    protected readonly EventCache _eventCache;

    public EventLoggerEfWriteStorage(DbGlobalContext storage, IEventLogContext eventLogContext, EventCache eventCache)
    {
        _storage = storage;
        _eventLogContext = eventLogContext;
        _eventCache = eventCache;
    }

    public virtual void LogEvent(EventDto @event)
    {
        _eventCache.Add(@event.ToEvent());
    }

    public void LogEvent(Func<IEventLogContext, EventDto> eventFunc)
    {
        LogEvent(eventFunc(_eventLogContext));
    }

    public virtual async Task FlushAsync()
    {
        if (_eventCache.IsEmpty()) return;

        _storage.ApplicationEvents.AddRange(_eventCache.GetEvents());
        await _storage.SaveChangesAsync();
        
        _eventCache.Clear();
    }
}