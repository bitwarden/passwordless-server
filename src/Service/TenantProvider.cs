#nullable enable

namespace Passwordless.Service;

public class TenantProvider : ITenantProvider
{
    public string Tenant { get; set; }
}