using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Constants;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
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
        var group = app.MapGroup("/signin")
            .RequireCors("default")
            .WithTags(OpenApiTags.SignIn);

        group.MapPost("/generate-token", async (
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
            .WithParameterValidation();

        group.MapPost("/begin", async (
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

        group.MapPost("/complete", async (
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
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        group.MapPost("/verify", async (
                SignInVerifyDTO payload,
                IFido2Service fido2Service
            ) =>
            {
                var result = await fido2Service.SignInVerifyAsync(payload);

                return Ok(result);
            })
            .RequireSecretKey(SecretKeyScopes.TokenVerify);

        app.MapPost("/stepup", async (
                StepUpTokenRequest request,
                ITokenService tokenService,
                TimeProvider timeProvider,
                IEventLogger eventLogger) =>
            {
                
                var token = await tokenService.EncodeTokenAsync(new StepUpToken
                {
                    ExpiresAt = default,
                    TokenId = default,
                    Type = null,
                    UserId = null,
                    CreatedAt = default,
                    RpId = null,
                    Origin = null,
                    Success = false,
                    Device = null,
                    Country = null
                }, "verify_");


                eventLogger.LogStepUpTokenCreated(request);

                return Ok(token);
            })
            .WithParameterValidation()
            .RequirePublicKey()
            .RequireCors();
    }
}