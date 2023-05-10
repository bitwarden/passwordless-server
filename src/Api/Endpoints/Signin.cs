using System.Security.Claims;
using ApiHelpers;
using Passwordless.Api.Authorization;
using Passwordless.Service;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Server.Endpoints;

public static class SigninEndpoints
{
    public static void MapSigninEndpoints(this WebApplication app)
    {

        app.MapPost("/signin/begin", async (SignInBeginDTO payload, ClaimsPrincipal user, ITenantStorage storage) =>
        {
            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);

            var result = await service.SignInBegin(payload);

            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/complete", async (SignInCompleteDTO payload, ClaimsPrincipal user, HttpRequest request, ITenantStorage storage) =>
        {
            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);

            var (deviceInfo, country) = Helpers.GetDeviceInfo(request);
            var result = await service.SignInComplete(payload, deviceInfo, country);

            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/verify", async (SignInVerifyDTO payload, ClaimsPrincipal user, ITenantStorage storage) =>
        {
            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);

            var result = await service.SignInVerify(payload);

            return Ok(result);
        })
            .RequireSecretKey()
            .RequireCors("default");
    }
}