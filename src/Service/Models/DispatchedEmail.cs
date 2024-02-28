namespace Passwordless.Service.Models;

public class DispatchedEmail : PerTenant
{
    public required Guid Id { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required string UserId { get; set; }

    public required string EmailAddress { get; set; }

    public required string LinkTemplate { get; set; }

    public virtual AccountMetaInformation? Application { get; set; }
}