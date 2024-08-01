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
            var actualOrganizationId = Convert.ToInt32(httpContext.User.FindFirst(CustomClaimTypes.OrgId)!.Value);

            if (context.Parameters.GetValue<int>("Organization") == actualOrganizationId)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
}