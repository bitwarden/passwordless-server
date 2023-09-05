namespace Passwordless.Common.AuditLog.Enums;

public enum AuditEventType
{
    ApiAuthUserRegistered = 1000,
    AdminOrganizationCreated = 7000,
    AdminSendAdminInvite = 7001,
    AdminMagicLinkLogin = 7002,
}