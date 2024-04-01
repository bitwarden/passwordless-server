namespace Passwordless.Common.Models.Apps;

public class GetAuthenticationScopesResult
{
    public IEnumerable<AuthenticationConfiguration> Scopes { get; set; } = new List<AuthenticationConfiguration>();
}