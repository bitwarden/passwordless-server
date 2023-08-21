using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.Models;

public class SetFeaturesDto
{
    [Range(0, 365)]
    public int AuditLoggingRetentionPeriod { get; set; }
}