using System.Security.Claims;

namespace Passwordless.AdminConsole.Helpers;

public static class ClaimsExtension
{
    public static int? GetOrgId(this ClaimsPrincipal user)
    {
        var orgIdStr = user.FindFirstValue("OrgId");
        if (orgIdStr == null) return null;
        int orgId = int.Parse(orgIdStr);
        return orgId;
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email);
    }
}