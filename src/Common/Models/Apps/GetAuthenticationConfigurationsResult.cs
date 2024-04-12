namespace Passwordless.Common.Models.Apps;

public class GetAuthenticationConfigurationsResult
{
    public IEnumerable<AuthenticationConfiguration> Configurations { get; set; } = new List<AuthenticationConfiguration>();
}