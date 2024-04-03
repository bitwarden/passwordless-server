namespace Passwordless.Service.Models;

public class UserSummary
{
    public string UserId { get; set; }
    public int AliasCount { get; set; }
    public List<string> Aliases { get; set; } = [];
    public int CredentialsCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
}