using Passwordless.Models;

namespace Passwordless.AdminConsole.EventLog.DTOs;

public record OrganizationEventLogResponse(int OrganizationId, IEnumerable<EventLogEvent> Events);

public record ApplicationEventLogResponse(string TenantId, IEnumerable<EventLogEvent> Events, int TotalEventCount);

public record EventLogEvent(DateTime PerformedAt, string EventType, string Message, string Severity, string PerformedBy, string Subject, string ApiKeyId);

public static class EventLogConversions
{
    public static ApplicationEventLogResponse ToDto(this GetEventLogResponse eventLogResponse) =>
        new(eventLogResponse.TenantId, eventLogResponse.Events.Select(ToDto), eventLogResponse.TotalEventCount);

    public static EventLogEvent ToDto(this ApplicationEvent applicationEvent) =>
        new(applicationEvent.PerformedAt,
            applicationEvent.EventType,
            applicationEvent.Message,
            applicationEvent.Severity,
            string.Empty,
            applicationEvent.Subject,
            applicationEvent.ApiKeyId);
}