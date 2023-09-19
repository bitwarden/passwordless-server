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
            DateTime.UtcNow);

    public static OrganizationEventDto CreateLoginViaMagicLinkEvent(ConsoleAdmin user) =>
        new("System",
            AuditEventType.AdminMagicLinkLogin,
            $"Login email sent for {user.Id} to {user.Email}.",
            Severity.Informational,
            user.Id,
            user.OrganizationId,
            DateTime.UtcNow);

    public static OrganizationEventDto DeleteAdminEvent(ConsoleAdmin performedBy, ConsoleAdmin deletedAdmin, DateTime performedAt) =>
        new(performedBy.Id,
            AuditEventType.AdminDeleteAdmin,
            $"Deleted admin {deletedAdmin.Name}",
            Severity.Informational,
            deletedAdmin.Id,
            performedBy.OrganizationId,
            performedAt);

    public static OrganizationEventDto InviteAdminEvent(ConsoleAdmin performedBy, string invitedEmail, DateTime performedAt) =>
        new(performedBy.Id,
            AuditEventType.AdminSendAdminInvite,
            $"Sent admin invite to {invitedEmail}",
            Severity.Informational,
            performedBy.OrganizationId.ToString(),
            performedBy.OrganizationId,
            performedAt);
    
    public static OrganizationEventDto CancelAdminInviteEvent(ConsoleAdmin performedBy, string invitedEmail, DateTime performedAt) =>
        new(performedBy.Id,
            AuditEventType.AdminCancelAdminInvite,
            $"Cancel admin invite for {invitedEmail}",
            Severity.Informational,
            performedBy.OrganizationId.ToString(),
            performedBy.OrganizationId,
            performedAt);

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
            OrganizationId = dto.OrganizationId
        };

    public static OrganizationEventDto ToDto(this OrganizationAuditEvent auditEvent) =>
        new(auditEvent.PerformedBy,
            auditEvent.EventType,
            auditEvent.Message,
            auditEvent.Severity,
            auditEvent.Subject,
            auditEvent.OrganizationId,
            auditEvent.PerformedAt);

    public static AuditLogEvent ToResponse(this OrganizationEventDto dto) =>
        new(dto.PerformedAt,
            dto.EventType.ToString(),
            dto.Message,
            dto.Severity.ToString(),
            dto.PerformedBy,
            dto.Subject);
}