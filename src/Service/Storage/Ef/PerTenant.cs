namespace Passwordless.Service.Storage.Ef;

public abstract class PerTenant
{
    public string Tenant { get; set; }
}