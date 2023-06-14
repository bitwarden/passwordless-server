namespace Passwordless.AspNetCore;

/// <summary>
/// 
/// </summary>
public class PasswordlessRegisterOptions
{
    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true" />
    /// </remarks>
    public bool Discoverable { get; set; } = true;

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <c>Preferred</c>
    /// </remarks>
    public string UserVerification { get; set; } = "Preferred";

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <c>None</c>
    /// </remarks>
    public string Attestation { get; set; } = "None";

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    // TODO: Maybe we make this nullable
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <c>any</c>
    /// </remarks>
    public string AuthenticationType { get; set; } = "any";

    /// <summary>
    /// Gets or sets whether to hash given aliases.
    /// </summary>
    public bool AliasHashing { get; set; } = true;
}
