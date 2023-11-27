#nullable enable


namespace Passwordless.Service;

public interface ITenantProvider
{
    public string Tenant { get; set; }
    public TimeProvider TimeProvider { get; set; }
}