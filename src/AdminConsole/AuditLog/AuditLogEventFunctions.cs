using AdminConsole.Identity;
using AdminConsole.Models;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.AuditLog.Models;
using Passwordless.Common.AuditLog.Enums;

namespace Passwordless.AdminConsole.AuditLog;

public static class AuditLogEventFunctions
{
    public static OrganizationEventDto CreateOrganizationCreatedEvent(Organization organization, ConsoleAdmin performedBy) =>
        new(performedBy.Id,
            AuditEventType.AdminOrganizationCreated,
            $"Organization {organization.Name} created by {performedBy.Name}.",
            Severity.Informational,
            organization.Id.ToString(),
            organization.Id,
            "");

    public static OrganizationEventDto CreateLoginViaMagicLinkEvent(ConsoleAdmin user) =>
        new("System",
            AuditEventType.AdminMagicLinkLogin,
            $"Login email sent for {user.Id} to {user.Email}.",
            Severity.Informational,
            user.Id,
            user.OrganizationId,
            "");

    public static OrganizationAuditEvent ToNewEvent(this OrganizationEventDto dto) =>
        new()
        {
            Id = Guid.NewGuid(),
            PerformedAt = dto.PerformedAt,
            EventType = dto.EventType,
            Message = dto.Message,
            Severity = dto.Severity,
            PerformedBy = dto.PerformedBy,
            Subject = dto.Subject,
            OrganizationId = dto.OrganizationId,
            ManagementKeyId = dto.ManagementKeyId
        };

    public static OrganizationEventDto ToDto(this OrganizationAuditEvent auditEvent) =>
        new(auditEvent.PerformedBy,
            auditEvent.EventType,
            auditEvent.Message,
            auditEvent.Severity,
            auditEvent.Subject,
            auditEvent.OrganizationId,
            auditEvent.ManagementKeyId);

    public static AuditLogEvent ToResponse(this OrganizationEventDto dto) =>
        new(dto.PerformedAt,
            dto.EventType.ToString(),
            dto.Message,
            dto.Severity.ToString(),
            dto.PerformedBy,
            dto.Subject,
            dto.ManagementKeyId);
}