using Passwordless.Api.Authorization;
using Passwordless.Common.Constants;
using Passwordless.Service.Features;
using Passwordless.Service.MagicLinks;
using Passwordless.Service.MagicLinks.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class MagicEndpoints
{
    public static void MapMagicEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/magic-link/send", async (
                SendMagicLinkRequest request,
                IFeatureContextProvider provider,
                MagicLinkService magicLinkService
            ) =>
            {
                // check if generate setting is on
                if (!(await provider.UseContext()).IsMagicLinksEnabled) return Forbid();

                await magicLinkService.SendMagicLink(request.ToDto());

                return Ok(new SendMagicLinkResponse());
            })
            .WithParameterValidation()
            .RequireAuthorization(SecretKeyScopes.TokenVerify)
            .RequireCors("default");
    }
}