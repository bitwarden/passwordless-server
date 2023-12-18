using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Models;

public record FeaturesContext(
    bool EventLoggingIsEnabled,
    int EventLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt,
    long? MaxUsers,
    bool IsGenerateSignInTokenEndpointEnabled)
{
    public static FeaturesContext FromDto(AppFeatureResponse dto)
    {
        return new FeaturesContext(dto.EventLoggingIsEnabled, dto.EventLoggingRetentionPeriod, dto.DeveloperLoggingEndsAt, dto.MaxUsers, dto.IsGenerateSignInTokenEndpointEnabled);
    }
}