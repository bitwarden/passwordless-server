namespace Passwordless.Service.Models;

public class AppFeatureDto
{
    public bool AuditLoggingIsEnabled { get; set; }
    public int AuditLoggingRetentionPeriod { get; set; }
    public DateTime? DeveloperLoggingEndsAt { get; set; }
    public int? MaxUsers { get; set; }

    public static AppFeatureDto FromEntity(AppFeature entity)
    {
        if (entity == null) return null;
        var dto = new AppFeatureDto
        {
            AuditLoggingIsEnabled = entity.AuditLoggingIsEnabled,
            AuditLoggingRetentionPeriod = entity.AuditLoggingRetentionPeriod,
            DeveloperLoggingEndsAt = entity.DeveloperLoggingEndsAt,
            MaxUsers = entity.MaxUsers
        };
        return dto;
    }
}