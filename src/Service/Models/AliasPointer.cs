namespace Passwordless.Service.Models;

public class AliasPointer : PerTenant
{
    public required string UserId { get; set; }
    public required string Alias { get; set; }
    public string? Plaintext { get; set; }
}