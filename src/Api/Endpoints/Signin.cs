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

        app.MapPost("/signin/begin", async (SignInBeginDTO payload, IFido2ServiceFactory fido2ServiceFactory) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var result = await fido2Service.SignInBegin(payload);

            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/complete", async (SignInCompleteDTO payload, HttpRequest request, IFido2ServiceFactory fido2ServiceFactory) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var (deviceInfo, country) = Helpers.GetDeviceInfo(request);
            var result = await fido2Service.SignInComplete(payload, deviceInfo, country);

            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/verify", async (SignInVerifyDTO payload, IFido2ServiceFactory fido2ServiceFactory) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var result = await fido2Service.SignInVerify(payload);

            return Ok(result);
        })
            .RequireSecretKey()
            .RequireCors("default");
    }
}
