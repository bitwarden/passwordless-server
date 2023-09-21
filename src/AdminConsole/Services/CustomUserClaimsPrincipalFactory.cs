using System.Security.Claims;
using AdminConsole.Db;
using AdminConsole.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AdminConsole.Services;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ConsoleAdmin>
{
    private readonly ConsoleDbContext _context;

    public CustomUserClaimsPrincipalFactory(
        UserManager<ConsoleAdmin> userManager,
        IOptions<IdentityOptions> optionsAccessor,
        ConsoleDbContext context
    )
        : base(userManager, optionsAccessor)
    {
        _context = context;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ConsoleAdmin user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("OrgId", user.OrganizationId.ToString()));

        // add apps
        List<string> apps = await _context.Applications.Where(a => a.OrganizationId == user.OrganizationId)
            .Select(a => a.Id).ToListAsync();

        foreach (var appId in apps)
        {
            identity.AddClaim(new Claim("AppId", appId));
        }

        return identity;
    }
}