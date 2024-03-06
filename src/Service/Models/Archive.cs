namespace Passwordless.Service.Models;

public class Archive : PerTenant
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Entity { get; set; }

    public string Data { get; set; }

    public AccountMetaInformation Application { get; set; }
}