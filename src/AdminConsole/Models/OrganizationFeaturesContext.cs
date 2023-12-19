namespace Passwordless.AdminConsole.Models;

public record OrganizationFeaturesContext(
    bool EventLoggingIsEnabled,
    int EventLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt,
    long? MaxUsers);