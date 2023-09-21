using Passwordless.AdminConsole.Models.DTOs;

namespace AdminConsole.Models;

public record FeaturesContext(
    bool AuditLoggingIsEnabled,
    int AuditLoggingRetentionPeriod,
    DateTime? DeveloperLoggingEndsAt)
{
    public static FeaturesContext FromDto(AppFeatureDto dto)
    {
        return new FeaturesContext(dto.AuditLoggingIsEnabled, dto.AuditLoggingRetentionPeriod, dto.DeveloperLoggingEndsAt);
    }
}