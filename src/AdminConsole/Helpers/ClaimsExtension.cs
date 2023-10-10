using System.Security.Claims;

namespace Passwordless.AdminConsole.Helpers;

public static class ClaimsExtension
{
    public static int GetOrgId(this ClaimsPrincipal user)
    {
        int orgId = int.Parse(user.FindFirstValue("OrgId"));
        return orgId;
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email);
    }
}