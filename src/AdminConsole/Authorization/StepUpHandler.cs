using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Passwordless.AdminConsole.Authorization;

public class StepUpRequirement(string policyName) : IStepUpRequirement
{
    public string Name => policyName;
};

public interface IStepUpRequirement : IAuthorizationRequirement
{
    public string Name { get; }
}

public class StepUpHandler(StepUpPurpose purpose, TimeProvider timeProvider) : AuthorizationHandler<IStepUpRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IStepUpRequirement requirement)
    {
        if (context.User.Identity is not { IsAuthenticated: true }) return Task.CompletedTask;

        if (context.User.HasClaim(MatchesClaim(requirement))
            && IsNotExpired(GetExpiration(context.User.FindFirst(MatchesClaim(requirement))!)))
        {
            context.Succeed(requirement);
        }
        else
        {
            purpose.Purpose = requirement.Name;
            context.Fail();
        }

        return Task.CompletedTask;
    }

    private static Predicate<Claim> MatchesClaim(IStepUpRequirement requirement) =>
        claim => claim.Type == requirement.Name;

    private bool IsNotExpired(DateTime expiration) => expiration > timeProvider.GetUtcNow().UtcDateTime;

    private static DateTime GetExpiration(Claim claim) =>
        DateTime.Parse(claim.Value, null, DateTimeStyles.RoundtripKind);
}