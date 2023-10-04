using Passwordless.AdminConsole.EventLog.DTOs;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public interface IEventLogger
{
    void LogEvent(OrganizationEventDto @event);
    Task FlushAsync();
}