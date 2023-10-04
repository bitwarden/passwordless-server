using Passwordless.Common.EventLog.Enums;

namespace Passwordless.AdminConsole.EventLog.DTOs;

public record OrganizationEventDto(
    string PerformedBy,
    EventType EventType,
    string Message,
    Severity Severity,
    string Subject,
    int OrganizationId,
    DateTime PerformedAt);