using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Passwordless.Api.Authorization;
using Passwordless.Api.Helpers;
using Passwordless.Api.OpenApi;
using Passwordless.Common.MagicLinks.Models;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.MagicLinks;
using Passwordless.Service.MagicLinks.Extensions;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class MagicEndpoints
{
    public const string RateLimiterPolicy = nameof(MagicEndpoints);

    /// <summary>
    /// Adds rate limiting policy for magic link endpoints.
    /// </summary>
    public static void AddMagicRateLimiterPolicy(this RateLimiterOptions builder) =>
        builder.AddPolicy(RateLimiterPolicy, context =>
        {
            var tenant = context.User.FindFirstValue(CustomClaimTypes.AccountName) ?? "<global>";

            var isRateLimitBypassed =
                context.RequestServices.GetRequiredService<IConfiguration>().IsRateLimitBypassEnabled() &&
                context.Request.IsRateLimitBypassRequested();

            if (isRateLimitBypassed)
                return RateLimitPartition.GetNoLimiter(tenant);

            return RateLimitPartition.GetFixedWindowLimiter(tenant, _ =>
                new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(5),
                    PermitLimit = 10,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }
            );
        });

    /// <summary>
    /// Maps magic link endpoints.
    /// </summary>
    public static void MapMagicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/magic-link")
            .RequireCors("default")
            .RequireSecretKey()
            .WithTags(OpenApiTags.MagicLinks);

        group.MapPost("/send", SendMagicLinkAsync)
            .WithParameterValidation()
            .RequireRateLimiting(RateLimiterPolicy);
    }

    /// <summary>
    /// Sends an e-mail containing a magic link template allowing users to login.
    /// </summary>
    /// <remarks>
    /// Warning: Verify the e-mail address matches the user identifier in your backend.
    /// </remarks>
    public static async Task<IResult> SendMagicLinkAsync(
        [FromBody] SendMagicLinkRequest request,
        [FromServices] IFeatureContextProvider provider,
        [FromServices] MagicLinkService magicLinkService)
    {
        if (!(await provider.UseContext()).IsMagicLinksEnabled)
            throw new ApiException("You must enable Magic Links in settings in order to use this feature.", 403);

        await magicLinkService.SendMagicLinkAsync(request.ToDto());

        return NoContent();
    }
}