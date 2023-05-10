namespace Passwordless.Net;

public class RegisterOptions
{
    public required string UserId { get; set; }
    public string? DisplayName { get; set; }
    public required string Username { get; set; }
    public string Attestation { get; set; }
    public string AuthenticatorType { get; set; }
    public bool Discoverable { get; set; }
    public string UserVerification { get; set; }
    public HashSet<string>? Aliases { get; set; }
    public bool AliasHashing { get; set; }

    public DateTime ExpiresAt { get; set; }
}