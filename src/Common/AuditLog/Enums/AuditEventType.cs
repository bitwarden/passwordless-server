namespace Passwordless.Common.AuditLog.Enums;

public enum AuditEventType
{
    Unknown = 0,
    ApiAuthUserRegistered = 1000,
    ApiAuthPasskeyRegistrationBegan = 1001,
    ApiAuthPasskeyRegistrationCompleted = 1002,
    ApiUserSetAliases = 1100,
    ApiUserDeleteCredential = 1101,
    ApiUserSignInBegan = 1102,
    ApiUserSignInCompleted = 1103,
    ApiUserSignInVerified = 1104,
    ApiUserDeleted = 1105,
    ApiManagementAppMarkedForDeletion = 1200,
    ApiManagementAppDeletionCancelled = 1201,
    ApiManagementAppCreated = 1202,
    ApiManagementAppFrozen = 1203,
    ApiManagementAppUnfrozen = 1204,
    AdminOrganizationCreated = 7000,
    AdminSendAdminInvite = 7001,
    AdminMagicLinkLogin = 7002,
    AdminDeleteAdmin = 7003,
    AdminCancelAdminInvite = 7004,
}