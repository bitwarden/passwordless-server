namespace Passwordless.Service.Models;

public sealed class ManageFeaturesDto : SetFeaturesDto
{
    public bool EventLoggingIsEnabled { get; set; }
}