namespace Passwordless.Service.Models;

public class Authenticator : PerTenant
{
    /// <summary>
    /// The authenticator attestation GUID.
    /// </summary>
    public Guid AaGuid { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When <value>true</value>, only whitelisted authenticators are allowed to be used during registration.
    /// </summary>
    public bool IsAllowed { get; set; }

    public AppFeature? AppFeature { get; set; }
}