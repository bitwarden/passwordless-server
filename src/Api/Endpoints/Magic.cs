using Passwordless.Api.Authorization;
using Passwordless.Common.Constants;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
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
                if (!(await provider.UseContext()).IsMagicLinksEnabled)
                    throw new ApiException("You must enable Magic Links setting to order to use this feature.", 403);

                await magicLinkService.SendMagicLink(request.ToDto());

                return Ok(new SendMagicLinkResponse());
            })
            .WithParameterValidation()
            .RequireAuthorization(SecretKeyScopes.TokenVerify)
            .RequireCors("default");
    }
}