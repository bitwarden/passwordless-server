namespace Passwordless.Common.Models.Apps;

public class DeleteAuthenticationConfigurationRequest
{
    public required string Purpose { get; set; }
    public required string PerformedBy { get; set; }
}