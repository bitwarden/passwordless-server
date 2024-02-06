namespace Passwordless.Service.Models;

public class DispatchedEmail : PerTenant
{
    public required Guid Id { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public required string UserId { get; set; }

    public required string EmailAddress { get; set; }

    public virtual AccountMetaInformation? Application { get; set; }
}