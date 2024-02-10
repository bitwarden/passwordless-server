#nullable enable


namespace Passwordless.Service;

public interface ITenantProvider
{
    public string Tenant { get; }
}