namespace Passwordless.Common.Models.Apps;

public record AppFeatureResponse(
    bool EventLoggingIsEnabled,
    int EventLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt,
    long? MaxUsers,
    AttestationTypes Attestation,
    bool IsGenerateSignInTokenEndpointEnabled);