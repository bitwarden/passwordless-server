using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Services;

public class CustomUserClaimsPrincipalFactory<TDbContext> : UserClaimsPrincipalFactory<ConsoleAdmin>
    where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    public CustomUserClaimsPrincipalFactory(
        UserManager<ConsoleAdmin> userManager,
        IOptions<IdentityOptions> optionsAccessor,
        IDbContextFactory<TDbContext> dbContextFactory
    )
        : base(userManager, optionsAccessor)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ConsoleAdmin user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("OrgId", user.OrganizationId.ToString()));

        // add apps
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        List<string> apps = await db.Applications.Where(a => a.OrganizationId == user.OrganizationId)
            .Select(a => a.Id).ToListAsync();

        foreach (var appId in apps)
        {
            identity.AddClaim(new Claim("AppId", appId));
        }

        return identity;
    }
}