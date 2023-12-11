using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.Common.EventLog.Enums;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class NoOpEventLogger : IEventLogger
{
    public void LogEvent(OrganizationEventDto @event)
    {
    }

    public void LogEvent(string performedBy, EventType eventType, string message, Severity severity, string subject,
        int organizationId, DateTime performedAt)
    {
    }

    public Task FlushAsync() => Task.CompletedTask;

    public static NoOpEventLogger Instance { get; } = new();
}