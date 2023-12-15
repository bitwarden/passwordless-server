namespace Passwordless.Common.Models.Apps;

public record AppFeatureResponse(
    bool EventLoggingIsEnabled,
    int EventLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt,
    long? MaxUsers,
    bool SignInTokenEndpointEnabled);