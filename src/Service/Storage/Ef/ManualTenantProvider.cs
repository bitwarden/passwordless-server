namespace Passwordless.Service.Storage.Ef;

public class ManualTenantProvider : ITenantProvider
{
    public ManualTenantProvider(string tenant, TimeProvider timeProvider)
    {
        TimeProvider = timeProvider;
        Tenant = tenant;
    }

    public string Tenant { get; set; }
    public TimeProvider TimeProvider { get; set; }
}