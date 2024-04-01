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

        group.MapGet("/scopes", GetAuthenticationScopesAsync).RequireSecretKey();

        group.MapPost("/scopes", CreateAuthenticationScopeAsync).RequireSecretKey();

        group.MapPut("/scopes", SetAuthenticationScopeAsync).RequireSecretKey();

    }

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

    /// <summary>
    /// A list of authentication scope configurations for your application. This will include the two default scopes of SignIn and StepUp.
    /// </summary>
    /// <returns>Result containing a list of AuthenticationConfigurations</returns>
    [ProducesResponseType(typeof(GetAuthenticationScopesResult), StatusCodes.Status200OK)]
    public static async Task<IResult> GetAuthenticationScopesAsync(
        [FromServices] IAuthenticationScopeService authenticationScopeService)
    {
        var configurations = await authenticationScopeService.GetAuthenticationScopesAsync();

        return Ok(new GetAuthenticationScopesResult
        {
            Scopes = configurations
                .Select(x =>
                    new AuthenticationConfiguration(
                        x.Purpose.Value,
                        Convert.ToInt32(x.TimeToLive.TotalSeconds),
                        x.UserVerificationRequirement.ToEnumMemberValue()))
        });
    }


    public static Task CreateAuthenticationScopeAsync(
        [FromBody] SetAuthenticationScopeRequest request,
        [FromServices] IAuthenticationScopeService authenticationScopeService)
    {
        throw new NotImplementedException();
    }

    public static Task SetAuthenticationScopeAsync(
        [FromBody] SetAuthenticationScopeRequest request,
        [FromServices] IAuthenticationScopeService authenticationScopeService)
    {
        throw new NotImplementedException();
    }
}