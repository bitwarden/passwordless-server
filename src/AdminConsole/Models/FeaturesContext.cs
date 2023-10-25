using Passwordless.AdminConsole.Models.DTOs;

namespace Passwordless.AdminConsole.Models;

public record FeaturesContext(
    bool EventLoggingIsEnabled,
    int EventLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt)
{
    public static FeaturesContext FromDto(AppFeatureDto dto)
    {
        return new FeaturesContext(dto.EventLoggingIsEnabled, dto.EventLoggingRetentionPeriod, dto.DeveloperLoggingEndsAt);
    }
}