namespace Passwordless.Service.Models;

public class PeriodicCredentialReport : PerTenant
{
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The number of credentials in the tenant.
    /// </summary>
    public int Credentials { get; set; }

    /// <summary>
    /// The number of users in the tenant.
    /// </summary>
    public int Users { get; set; }

    public virtual AccountMetaInformation Application { get; init; }

}