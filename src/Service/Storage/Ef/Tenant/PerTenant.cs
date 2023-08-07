namespace Passwordless.Service.Storage.Ef.Tenant;

public abstract class PerTenant
{
    public string Tenant { get; set; }
}