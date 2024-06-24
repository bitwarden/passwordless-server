using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi;
using Passwordless.Common.MagicLinks.Models;
using Passwordless.Common.Overrides;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.MagicLinks;
using Passwordless.Service.MagicLinks.Extensions;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class MagicEndpoints
{
    /// <summary>
    /// Name of the Magic Links Rate Limiter Policy
    /// </summary>
    public const string RateLimiterPolicy = nameof(MagicEndpoints);

    /// <summary>
    /// Adds a rate limiter policy for the MagicEndpoints. Each tenant will have its own partition.
    /// </summary>
    public static void AddMagicRateLimiterPolicy(this RateLimiterOptions builder) =>
        builder.AddPolicy(RateLimiterPolicy, context =>
        {
            var tenant = context.User.FindFirstValue(CustomClaimTypes.AccountName) ?? "<global>";

            var applicationOverridesOptions = context.RequestServices
                .GetRequiredService<IOptionsSnapshot<ApplicationOverridesOptions>>();

            var applicationOverrides = applicationOverridesOptions.Value.GetApplication(tenant);
            if (applicationOverrides.IsRateLimitBypassEnabled)
                return RateLimitPartition.GetNoLimiter(tenant);

            return RateLimitPartition.GetFixedWindowLimiter(tenant, _ =>
                new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(5), PermitLimit = 10, QueueLimit = 0, AutoReplenishment = true }
            );
        });

    /// <summary>
    /// Maps the magic link endpoints.
    /// </summary>
    public static void MapMagicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/magic-links")
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
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.TooManyRequests, MediaTypeNames.Application.ProblemJson)]
    [ExternalDocs("https://docs.passwordless.dev/guide/api.html#magic-links-send")]
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