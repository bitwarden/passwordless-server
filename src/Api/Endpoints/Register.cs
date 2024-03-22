using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Constants;
using Passwordless.Service;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class RegisterEndpoints
{
    public record RegisterTokenResponse(string Token);

    public static void MapRegisterEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/register")
            .RequireCors("default")
            .WithTags(OpenApiTags.Registration);

        group.MapPost("/token", RegisterAsync)
            .RequireSecretKey(SecretKeyScopes.TokenRegister)
            .WithParameterValidation();

        group.MapPost("/begin", RegisterBeginAsync)
            .RequirePublicKey(PublicKeyScopes.Register)
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        group.MapPost("/complete", RegisterCompleteAsync)
            .RequirePublicKey(PublicKeyScopes.Register)
            .WithParameterValidation()
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));
    }

    /// <summary>
    /// Registration (Step 1 - Server)
    /// </summary>
    public static async Task<IResult> RegisterAsync(
        [FromBody] RegisterToken request,
        [FromServices] IFido2Service fido2Service)
    {
        var result = await fido2Service.CreateRegisterTokenAsync(request);
        return Ok(new RegisterTokenResponse(result));
    }

    /// <summary>
    /// Registration (Step 2 - Client): Get credential creation options.
    /// </summary>
    public static async Task<IResult> RegisterBeginAsync(
        [FromBody] FidoRegistrationBeginDTO payload,
        [FromServices] IFido2Service fido2Service)
    {
        var result = await fido2Service.RegisterBeginAsync(payload);
        return Ok(result);
    }

    /// <summary>
    /// Registration (Step 3 - Client): Stores the created public key.
    /// </summary>
    public static async Task<IResult> RegisterCompleteAsync(
        [FromBody] RegistrationCompleteDTO payload,
        HttpRequest request,
        [FromServices] IFido2Service fido2Service)
    {
        var (deviceInfo, country) = request.GetDeviceInfo();
        var result = await fido2Service.RegisterCompleteAsync(payload, deviceInfo, country);

        // Avoid serializing the certificate
        return Ok(result);
    }
}