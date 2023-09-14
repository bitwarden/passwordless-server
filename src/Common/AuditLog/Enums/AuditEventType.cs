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
    ApiUserSetAliases = 1100,
    ApiManagementAppMarkedForDeletion = 1200,
    ApiManagementAppDeletionCancelled = 1201,
    ApiManagementAppCreated = 1202,
    ApiManagementAppFrozen = 1203,
    ApiManagementAppUnfrozen = 1204,
    AdminOrganizationCreated = 7000,
    AdminSendAdminInvite = 7001,
    AdminMagicLinkLogin = 7002,
}