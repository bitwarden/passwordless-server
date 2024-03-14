using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.MagicLinks;
using Passwordless.Service.MagicLinks.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class MagicEndpoints
{
    public const string RateLimiterPolicy = nameof(MagicEndpoints);

    public static void AddMagicRateLimiterPolicy(this RateLimiterOptions builder) =>
        builder.AddPolicy(RateLimiterPolicy, context =>
        {
            var tenant = context.User.FindFirstValue(CustomClaimTypes.AccountName) ?? "<global>";

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

    public static void MapMagicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/magic-link")
            .RequireCors("default")
            .RequireSecretKey()
            .WithTags(OpenApiTags.MagicLinks);

        group.MapPost("/send", async (
                SendMagicLinkRequest request,
                IFeatureContextProvider provider,
                MagicLinkService magicLinkService
            ) =>
            {
                if (!(await provider.UseContext()).IsMagicLinksEnabled)
                    throw new ApiException("You must enable Magic Links in settings in order to use this feature.", 403);

                await magicLinkService.SendMagicLinkAsync(request.ToDto());

                return NoContent();
            })
            .WithParameterValidation();
    }
}