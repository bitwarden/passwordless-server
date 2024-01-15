namespace Passwordless.Service.Models;

public class ApiKeyDesc : PerTenant
{
    public string Id { get; set; }

    [Obsolete]
    public string? AccountName { get; set; }
    public string ApiKey { get; set; }
    public string[] Scopes { get; set; }
    public bool IsLocked { get; set; }

    public DateTime? LastLockedAt { get; set; }
    public DateTime? LastUnlockedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string MaskedApiKey => ApiKey.Contains("public")
        ? $"{Tenant}:public:{Id.PadLeft(32, '*')}"
        : $"{Tenant}:secret:{Id.PadLeft(32, '*')}";
}