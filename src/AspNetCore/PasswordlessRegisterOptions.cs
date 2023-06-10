namespace Passwordless.AspNetCore;

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
    // TODO: Should we have some sort of default lifetime? and is this used?
    public TimeSpan? Expiration { get; set; }

    /// <summary>
    /// TODO: Fill in
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="null" />
    /// </remarks>
    public string? AuthenticationType { get; set; }
}
