namespace Passwordless.AdminConsole.EventLog.DTOs;

public record OrganizationEventLogResponse(IEnumerable<EventLogEvent> Events);

public record ApplicationEventLogResponse(string TenantId, IEnumerable<EventLogEvent> Events, int TotalEventCount);

public record EventLogEvent(DateTime PerformedAt, string EventType, string Message, string Severity, string PerformedBy, string Subject, string ApiKeyId);