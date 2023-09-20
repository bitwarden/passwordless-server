namespace Passwordless.AdminConsole.AuditLog.DTOs;

public record OrganizationAuditLogResponse(int OrganizationId, IEnumerable<AuditLogEvent> Events);

public record ApplicationAuditLogResponse(string TenantId, IEnumerable<AuditLogEvent> Events, int TotalEventCount);

public record AuditLogEvent(DateTime PerformedAt, string EventType, string Message, string Severity, string PerformedBy, string Subject, string ApiKeyAbbreviated);