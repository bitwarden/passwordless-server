using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog.Loggers;

public class EventCache
{
    private readonly List<ApplicationEvent> _events = new();
    public IEnumerable<ApplicationEvent> GetEvents() => _events;
    public bool IsEmpty() => !_events.Any();
    public void Add(ApplicationEvent applicationEvent) => _events.Add(applicationEvent);
    public void Clear() => _events.Clear();
}