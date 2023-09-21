using System.ComponentModel.DataAnnotations;

namespace Passwordless.Service.Models;

public class SetFeaturesDto
{
    [Range(0, 90)]
    public int AuditLoggingRetentionPeriod { get; set; }
}