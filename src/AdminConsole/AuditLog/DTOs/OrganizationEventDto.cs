using Passwordless.Common.AuditLog.Enums;

namespace Passwordless.AdminConsole.AuditLog.DTOs;

public record OrganizationEventDto(
    string PerformedBy,
    AuditEventType EventType,
    string Message,
    Severity Severity,
    string Subject,
    int OrganizationId,
    string ManagementKeyId,
    DateTime PerformedAt);