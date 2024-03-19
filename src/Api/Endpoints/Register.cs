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

        group.MapPost("/token", async (
                RegisterToken registerToken,
                IFido2Service fido2Service,
                CancellationToken token
            ) =>
            {
                var result = await fido2Service.CreateRegisterTokenAsync(registerToken);
                return Ok(new RegisterTokenResponse(result));
            })
            .RequireSecretKey(SecretKeyScopes.TokenRegister)
            .WithParameterValidation();

        group.MapPost("/begin", async (
                FidoRegistrationBeginDTO payload,
                IFido2Service fido2Service,
                CancellationToken token
            ) =>
            {
                var result = await fido2Service.RegisterBeginAsync(payload);
                return Ok(result);
            })
            .RequirePublicKey(PublicKeyScopes.Register)
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        group.MapPost("/complete", async (
                RegistrationCompleteDTO payload,
                HttpRequest request,
                IFido2Service fido2Service,
                CancellationToken token
            ) =>
            {
                var (deviceInfo, country) = request.GetDeviceInfo();
                var result = await fido2Service.RegisterCompleteAsync(payload, deviceInfo, country);

                // Avoid serializing the certificate
                return Ok(result);
            })
            .WithParameterValidation()
            .RequirePublicKey(PublicKeyScopes.Register)
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));
    }
}