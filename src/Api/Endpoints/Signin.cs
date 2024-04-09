using System.Net;
using System.Net.Mime;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Constants;
using Passwordless.Common.Models.Apps;
using Passwordless.Service;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;
using AuthenticationConfiguration = Passwordless.Common.Models.Apps.AuthenticationConfiguration;

namespace Passwordless.Api.Endpoints;

public static class SigninEndpoints
{
    public record SigninTokenResponse(string Token);

    public static void MapSigninEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/signin")
            .RequireCors("default")
            .WithOpenApi()
            .WithTags(OpenApiTags.SignIn);

        group.MapPost("/generate-token", GenerateTokenAsync)
            .RequireSecretKey(SecretKeyScopes.TokenVerify)
            .WithParameterValidation();

        group.MapPost("/begin", BeginAsync)
            .RequirePublicKey(PublicKeyScopes.Login)
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        group.MapPost("/complete", CompleteAsync)
            .RequirePublicKey(PublicKeyScopes.Login)
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        group.MapPost("/verify", VerifyAsync)
            .RequireSecretKey(SecretKeyScopes.TokenVerify);

        group.MapGet("/authentication-configurations", async (
                [FromServices] IAuthenticationConfigurationService service) =>
            {
                var configurations = await service.GetAuthenticationConfigurationsAsync();

                return Ok(new GetAuthenticationScopesResult
                {
                    Scopes = configurations
                        .Select(x =>
                            new AuthenticationConfiguration(
                                x.Purpose.Value,
                                Convert.ToInt32(x.TimeToLive.TotalSeconds),
                                x.UserVerificationRequirement.ToEnumMemberValue()))
                });
            })
            .WithSummary("A list of authentication scope configurations for your application. This will include the two default scopes of SignIn and StepUp.")
            .Produces<GetAuthenticationScopesResult>()
            .RequireSecretKey();

        group.MapPost("/authentication-configuration/new", async (
                [FromBody] SetAuthenticationScopeRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService,
                HttpRequest httpRequest) =>
            {
                await authenticationConfigurationService.CreateAuthenticationConfigurationAsync(new AuthenticationConfigurationDto
                {
                    Purpose = new SignInPurpose(request.Purpose),
                    UserVerificationRequirement = request.UserVerificationRequirement,
                    TimeToLive = request.TimeToLive,
                    Tenant = httpRequest.GetTenantName()!
                });

                return Created();
            })
            .WithSummary(
                "Creates or updates an authentication configuration for the sign-in process. In order to use this, it will have to be provided to the `stepup` client method via the purpose field")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapPost("/authentication-configuration", async (
                [FromBody] SetAuthenticationScopeRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService,
                HttpRequest httpRequest) =>
            {
                await authenticationConfigurationService.UpdateAuthenticationConfigurationAsync(new AuthenticationConfigurationDto
                {
                    Purpose = new SignInPurpose(request.Purpose),
                    UserVerificationRequirement = request.UserVerificationRequirement,
                    TimeToLive = request.TimeToLive,
                    Tenant = httpRequest.GetTenantName()!
                });

                return Ok();
            })
            .WithSummary(
                "Creates or updates an authentication configuration for the sign-in process. In order to use this, it will have to be provided to the `stepup` client method via the purpose field")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapGet("/authentication-configuration/{purpose}", async (
                [FromRoute] string purpose,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService) =>
            {
                var configuration = await authenticationConfigurationService.GetAuthenticationConfigurationAsync(purpose);
                return configuration is null
                    ? NotFound()
                    : Ok(configuration);
            })
            .WithSummary("Authentication configuration for the specified purpose.")
            .Produces<AuthenticationConfigurationDto>()
            .Produces(StatusCodes.Status404NotFound)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapDelete("/authentication-configuration", async (
                [FromBody] DeleteAuthenticationConfigurationRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService) =>
            {
                var configuration = await authenticationConfigurationService.GetAuthenticationConfigurationAsync(request.Purpose);

                if (configuration == null) return NotFound();

                await authenticationConfigurationService.DeleteAuthenticationConfigurationAsync(configuration);

                return Ok();
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithParameterValidation()
            .RequireSecretKey();
    }

    /// <summary>
    /// Manually generates an authentication token for the specified user, side-stepping the usual authentication flow.
    /// This approach can be used to implement a "magic link"-style login and other similar scenarios.
    /// </summary>
    [ProducesResponseType(typeof(SigninTokenRequest), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> GenerateTokenAsync(
        [FromBody] SigninTokenRequest signinToken,
        [FromServices] IFeatureContextProvider provider,
        [FromServices] IFido2Service fido2Service)
    {
        if (!(await provider.UseContext()).IsGenerateSignInTokenEndpointEnabled)
        {
            throw new ApiException("The 'POST /signin/generate-token' endpoint is disabled", 403);
        }

        var result = await fido2Service.CreateSigninTokenAsync(signinToken);

        return Ok(new SigninTokenResponse(result));
    }

    /// <summary>
    /// Signin (Step 1 - Client)
    /// </summary>
    [ProducesResponseType(typeof(SessionResponse<AssertionOptions>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> BeginAsync(
        [FromBody] SignInBeginDTO payload,
        [FromServices] IFido2Service fido2Service)
    {
        var result = await fido2Service.SignInBeginAsync(payload);

        return Ok(result);
    }

    /// <summary>
    /// Signin (Step 2 - Client)
    /// </summary>
    [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> CompleteAsync(
        [FromBody] SignInCompleteDTO payload,
        HttpRequest request,
        [FromServices] IFido2Service fido2Service)
    {
        var (deviceInfo, country) = request.GetDeviceInfo();
        var result = await fido2Service.SignInCompleteAsync(payload, deviceInfo, country);

        return Ok(result);
    }

    /// <summary>
    /// Signin (Step 3 - Server)
    /// </summary>
    [ProducesResponseType(typeof(VerifySignInToken), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> VerifyAsync(
        [FromBody] SignInVerifyDTO payload,
        [FromServices] IFido2Service fido2Service)
    {
        var result = await fido2Service.SignInVerifyAsync(payload);
        return Ok(result);
    }
}