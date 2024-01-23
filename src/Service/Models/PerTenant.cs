namespace Passwordless.Service.Models;

public abstract class PerTenant
{
    public required string Tenant { get; set; }
}