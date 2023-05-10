namespace Passwordless.Net;

public class PasswordlessUserSummary
{
    public string UserId { get; set; }
    public List<string> Aliases { get; set; }
    public int CredentialsCount { get; set; }
    public int AliasCount { get; set; }
    public DateTime LastUsedAt { get; set; }
}