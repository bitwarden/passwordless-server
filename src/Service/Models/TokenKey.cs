namespace Passwordless.Service.Models;

public class TokenKey : PerTenant
{
    public required string KeyMaterial { get; set; }
    public required int KeyId { get; set; }
    public required DateTime CreatedAt { get; set; }
}