using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Services;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ConsoleAdmin>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<ConsoleAdmin> userManager,
        IOptions<IdentityOptions> optionsAccessor
    )
        : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ConsoleAdmin user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(CustomClaimTypes.OrgId, user.OrganizationId.ToString()));
        return identity;
    }
}