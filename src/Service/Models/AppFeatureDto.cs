namespace Passwordless.Service.Models;

public class AppFeatureDto
{
    public bool EventLoggingIsEnabled { get; set; }
    public int EventLoggingRetentionPeriod { get; set; }
    public DateTime? DeveloperLoggingEndsAt { get; set; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; set; }

    public static AppFeatureDto FromEntity(AppFeature entity)
    {
        if (entity == null) return null;
        var dto = new AppFeatureDto
        {
            EventLoggingIsEnabled = entity.EventLoggingIsEnabled,
            EventLoggingRetentionPeriod = entity.EventLoggingRetentionPeriod,
            DeveloperLoggingEndsAt = entity.DeveloperLoggingEndsAt
        };
        return dto;
    }
}