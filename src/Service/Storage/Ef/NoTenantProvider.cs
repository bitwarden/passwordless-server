namespace Passwordless.Service.Storage.Ef;

public sealed class NoTenantProvider : ITenantProvider
{
    public string Tenant { get; set; }
}