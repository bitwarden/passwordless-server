namespace Passwordless.Common.AuditLog.Enums;

public enum AuditEventType
{
    Unknown = 0,
    ApiAuthUserRegistered = 1000,
    ApiAuthPasskeyRegistrationBegan = 1001,
    ApiAuthPasskeyRegistrationCompleted = 1002,
    ApiSignInBegan = 1003,
    ApiSignInCompleted = 1004,
    ApiSignInVerified = 1005,
    ApiManagementAppMarkedForDeletion = 1010,
    ApiManagementAppDeletionCanceled = 1011,
    AdminOrganizationCreated = 7000,
    AdminSendAdminInvite = 7001,
    AdminMagicLinkLogin = 7002,
}