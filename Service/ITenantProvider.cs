#nullable enable


namespace Service;

public interface ITenantProvider
{
    public string Tenant { get; set; }
}
