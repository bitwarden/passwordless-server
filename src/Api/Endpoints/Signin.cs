using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Service;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class SigninEndpoints
{
    private record SigninTokenRequest(string UserId);

    private record SigninTokenResponse(string Token);

    public static void MapSigninEndpoints(this WebApplication app)
    {
        app.MapPost("/signin/generate-token", async (
                SigninTokenRequest signinToken,
                IFido2ServiceFactory fido2ServiceFactory
            ) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync();
                var result = await fido2Service.CreateSigninToken(signinToken.UserId);

                return Ok(new SigninTokenResponse(result));
            })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapPost("/signin/begin", async (
                SignInBeginDTO payload,
                IFido2ServiceFactory fido2ServiceFactory
            ) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync();
                var result = await fido2Service.SignInBegin(payload);

                return Ok(result);
            })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/signin/complete", async (
                SignInCompleteDTO payload,
                HttpRequest request,
                IFido2ServiceFactory fido2ServiceFactory
            ) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync();
                var (deviceInfo, country) = request.GetDeviceInfo();
                var result = await fido2Service.SignInComplete(payload, deviceInfo, country);

                return Ok(result);
            })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/signin/verify", async (
                SignInVerifyDTO payload,
                IFido2ServiceFactory fido2ServiceFactory
            ) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync();
                var result = await fido2Service.SignInVerify(payload);

                return Ok(result);
            })
            .RequireSecretKey()
            .RequireCors("default");
    }
}