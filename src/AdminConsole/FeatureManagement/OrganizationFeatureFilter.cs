using Microsoft.FeatureManagement;
using Passwordless.AdminConsole.Authorization;

namespace Passwordless.AdminConsole.FeatureManagement;

[FilterAlias("Organization")]
public class OrganizationFeatureFilter : IFeatureFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationFeatureFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext!;

        if (httpContext.User.Identity is { IsAuthenticated: true })
        {
            var organizationIdClaim = httpContext.User.FindFirst(CustomClaimTypes.OrgId);
            if (organizationIdClaim == null)
            {
                throw new InvalidOperationException("User should have an organization ID claim.");
            }

            if (context.Parameters.GetValue<int>("Organization") == Convert.ToInt32(organizationIdClaim.Value))
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
}