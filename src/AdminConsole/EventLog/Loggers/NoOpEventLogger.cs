using Passwordless.AdminConsole.EventLog.DTOs;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class NoOpEventLogger : IEventLogger
{
    public void LogEvent(OrganizationEventDto @event)
    {
    }

    public Task FlushAsync() => Task.CompletedTask;

    public static NoOpEventLogger Instance { get; } = new();
}