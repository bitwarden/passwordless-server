using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog.Loggers;

public interface IEventLogger
{
    void LogEvent(EventDto @event);
    void LogEvent(Func<IEventLogContext, EventDto> eventFunc);
    Task FlushAsync();
}