using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.Models;

public sealed class ManageFeaturesDto : SetFeaturesDto
{
    public bool AuditLoggingIsEnabled { get; set; }

    [Range(0, int.MaxValue)]
    public int? MaxUsers { get; set; }
}