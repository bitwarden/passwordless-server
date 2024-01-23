namespace Passwordless.Service.Models;

public class Authenticator : PerTenant
{
    public Guid AaGuid { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public bool IsAllowed { get; set; }
    
    public AppFeature AppFeature { get; set; }
}