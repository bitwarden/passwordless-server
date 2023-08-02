namespace Passwordless.Net;

public class RegisterOptions
{
    public
#if NET6_0_OR_GREATER
    required
#endif
    string UserId { get; set; }

    public string? DisplayName { get; set; }

    public
#if NET6_0_OR_GREATER
    required
#endif
    string Username { get; set; }

    public string Attestation { get; set; }
    public string AuthenticatorType { get; set; }
    public bool Discoverable { get; set; }
    public string UserVerification { get; set; }
    public HashSet<string>? Aliases { get; set; }
    public bool AliasHashing { get; set; }

    public DateTime ExpiresAt { get; set; }
}
