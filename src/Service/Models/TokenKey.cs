using Passwordless.Service.Storage.Ef.Tenant;

namespace Passwordless.Service.Models;

public class TokenKey : PerTenant
{
    public string KeyMaterial { get; set; }
    public int KeyId { get; set; }
    public DateTime CreatedAt { get; set; }
}