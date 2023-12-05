namespace Passwordless.Service.Models;

public sealed class ManageFeaturesDto : SetFeaturesDto
{
    public bool EventLoggingIsEnabled { get; set; }

    /// <summary>
    /// Maximum number of individual users allowed to use the application
    /// </summary>
    public long? MaxUsers { get; set; }
}