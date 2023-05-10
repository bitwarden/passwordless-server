using System.ComponentModel.DataAnnotations;

namespace AdminConsole.Identity;

public class Invite
{
    [MaxLength(50)]
    public string HashedCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpireAt { get; set; }
    [MaxLength(50)]
    public string FromName { get; set; }
    [MaxLength(50)]
    public string FromEmail { get; set; }
    public int TargetOrgId { get; set; }
    [MaxLength(50)]
    public string TargetOrgName { get; set; }
    [MaxLength(50)]
    public string ToEmail { get; set; }
}