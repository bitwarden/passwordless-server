using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Models;

public record ApplicationFeatureContext(
    bool EventLoggingIsEnabled,
    int EventLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt,
    long? MaxUsers,
    bool IsGenerateSignInTokenEndpointEnabled,
    bool IsMagicLinksEnabled)
{
    public static ApplicationFeatureContext FromDto(AppFeatureResponse dto)
    {
        return new ApplicationFeatureContext(
            dto.EventLoggingIsEnabled,
            dto.EventLoggingRetentionPeriod,
            dto.DeveloperLoggingEndsAt,
            dto.MaxUsers,
            dto.IsGenerateSignInTokenEndpointEnabled,
            dto.IsMagicLinksEnabled);
    }
};