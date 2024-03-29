using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Models;

public record ApplicationFeatureContext(
    bool EventLoggingIsEnabled,
    int EventLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt,
    long? MaxUsers,
    bool AllowAttestation,
    bool IsGenerateSignInTokenEndpointEnabled,
    bool IsMagicLinksEnabled)
{
    public static ApplicationFeatureContext FromDto(AppFeatureResponse dto) =>
        new(
            dto.EventLoggingIsEnabled,
            dto.EventLoggingRetentionPeriod,
            dto.DeveloperLoggingEndsAt,
            dto.MaxUsers,
            dto.AllowAttestation,
            dto.IsGenerateSignInTokenEndpointEnabled,
            dto.IsMagicLinksEnabled);
}