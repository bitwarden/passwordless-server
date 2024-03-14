using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Common.Constants;
using Passwordless.Service;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class SigninEndpoints
{
    public record SigninTokenResponse(string Token);

    public static void MapSigninEndpoints(this WebApplication app)
    {
        app.MapPost("/signin/generate-token", async (
                SigninTokenRequest signinToken,
                IFeatureContextProvider provider,
                IFido2Service fido2Service
            ) =>
            {
                if (!(await provider.UseContext()).IsGenerateSignInTokenEndpointEnabled)
                {
                    throw new ApiException("The 'POST /signin/generate-token' endpoint is disabled", 403);
                }

                var result = await fido2Service.CreateSigninTokenAsync(signinToken);

                return Ok(new SigninTokenResponse(result));
            })
            .RequireSecretKey(SecretKeyScopes.TokenVerify)
            .RequireCors("default");

        app.MapPost("/signin/begin", async (
                SignInBeginDTO payload,
                IFido2Service fido2Service
            ) =>
            {
                var result = await fido2Service.SignInBeginAsync(payload);

                return Ok(result);
            })
            .RequirePublicKey(PublicKeyScopes.Login)
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/signin/complete", async (
                SignInCompleteDTO payload,
                HttpRequest request,
                IFido2Service fido2Service
            ) =>
            {
                var (deviceInfo, country) = request.GetDeviceInfo();
                var result = await fido2Service.SignInCompleteAsync(payload, deviceInfo, country);

                return Ok(result);
            })
            .RequirePublicKey(PublicKeyScopes.Login)
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/signin/verify", async (
                SignInVerifyDTO payload,
                IFido2Service fido2Service
            ) =>
            {
                var result = await fido2Service.SignInVerifyAsync(payload);

                return Ok(result);
            })
            .RequireSecretKey(SecretKeyScopes.TokenVerify)
            .RequireCors("default");
    }
}