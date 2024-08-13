using System.Security.Claims;

namespace Passwordless.AdminConsole.Tests.Factory;

public static class ClaimsPrincipalFactory
{
    public static ClaimsPrincipal CreateJohnDoe()
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, "John Doe"),
            new (ClaimTypes.Email, "johndoe@example.com"),
        };

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}