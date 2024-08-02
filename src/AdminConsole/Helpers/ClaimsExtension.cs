using System.Globalization;
using System.Security.Claims;
using Passwordless.AdminConsole.Authorization;

namespace Passwordless.AdminConsole.Helpers;

public static class ClaimsExtension
{
    /// <summary>
    /// Returns the organization ID of the user.
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The organization ID</returns>
    /// <exception cref="InvalidOperationException">
    /// Exception thrown when the user does not have an 'OrgId' claim.
    /// </exception>
    public static int? GetOrgId(this ClaimsPrincipal user)
    {
        var orgIdStr = user.FindFirstValue(CustomClaimTypes.OrgId);
        if (orgIdStr == null)
        {
            return null;
        }
        var orgId = int.Parse(orgIdStr, CultureInfo.InvariantCulture);
        return orgId;
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email);
    }

    public static string GetId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public static string GetName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
}