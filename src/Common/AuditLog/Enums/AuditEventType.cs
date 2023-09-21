namespace Passwordless.Common.AuditLog.Enums;

public enum AuditEventType
{
    Unknown = 0,
    ApiAuthUserRegistered = 1000,
    ApiManagementAppMarkedForDeletion = 1010,
    ApiManagementAppDeletionCanceled = 1011,
    AdminOrganizationCreated = 7000,
    AdminSendAdminInvite = 7001,
    AdminMagicLinkLogin = 7002,
}