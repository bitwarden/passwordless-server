using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Services;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ConsoleAdmin>
{
    private readonly ConsoleDbContext _db;

    public CustomUserClaimsPrincipalFactory(
        UserManager<ConsoleAdmin> userManager,
        IOptions<IdentityOptions> optionsAccessor,
        ConsoleDbContext db
    )
        : base(userManager, optionsAccessor)
    {
        _db = db;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ConsoleAdmin user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("OrgId", user.OrganizationId.ToString()));

        // add apps
        List<string> apps = await _db.Applications.Where(a => a.OrganizationId == user.OrganizationId)
            .Select(a => a.Id).ToListAsync();

        foreach (var appId in apps)
        {
            identity.AddClaim(new Claim("AppId", appId));
        }

        return identity;
    }
}