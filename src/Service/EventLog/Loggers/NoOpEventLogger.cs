using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog.Loggers;

public class NoOpEventLogger : IEventLogger
{
    public void LogEvent(EventDto @event) { }

    public void LogEvent(Func<IEventLogContext, EventDto> eventFunc) { }

    public Task FlushAsync() => Task.CompletedTask;

    public static NoOpEventLogger Instance { get; } = new();
}