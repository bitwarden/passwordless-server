using Passwordless.Common.AuditLog.Enums;
using Passwordless.Common.AuditLog.Models;

namespace Passwordless.Api.Models;

public class AuditEventRequest
{
    public string PerformedBy { get; init; }
    public DateTime PerformedAt { get; init; }
    public AuditEventType EventType { get; init; }
    public string Message { get; init; }
    public Severity Severity { get; init; }
    public string Subject { get; init; }
    public string? TenantId { get; init; }
    public int? OrganizationId { get; init; }
}

public static class AuditEventRequestExtensions
{
    public static AuditEventDto ToEvent(this AuditEventRequest request) => new()
    {
        PerformedAt = request.PerformedAt,
        Message = request.Message,
        PerformedBy = request.PerformedBy,
        TenantId = request.TenantId,
        OrganizationId = request.OrganizationId,
        EventType = request.EventType,
        Severity = request.Severity,
        Subject = request.Subject,

    };
}