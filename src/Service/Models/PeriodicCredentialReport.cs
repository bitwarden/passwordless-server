namespace Passwordless.Service.Models;

public class PeriodicCredentialReport : PerTenant
{
    public DateOnly CreatedAt { get; set; }

    /// <summary>
    /// The number of credentials in the tenant.
    /// </summary>
    public int CredentialsCount { get; set; }

    /// <summary>
    /// The number of users in the tenant.
    /// </summary>
    public int UsersCount { get; set; }

    public virtual AccountMetaInformation Application { get; init; }

}