using Passwordless.Api.Authorization;
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
        app.MapPost("/register/token", async (
                RegisterToken registerToken,
                IFido2Service fido2Service,
                CancellationToken token
            ) =>
            {
                var result = await fido2Service.CreateRegisterToken(registerToken);

                return Ok(new RegisterTokenResponse(result));
            })
            .RequireAuthorization(ApiKeyScopes.SecretTokenRegister)
            .RequireCors("default");

        app.MapPost("/register/begin", async (
                FidoRegistrationBeginDTO payload,
                IFido2Service fido2Service,
                CancellationToken token
            ) =>
            {
                var result = await fido2Service.RegisterBegin(payload);

                return Ok(result);
            })
            .RequireAuthorization(ApiKeyScopes.PublicRegister)
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/register/complete", async (
                RegistrationCompleteDTO payload,
                HttpRequest request,
                IFido2Service fido2Service,
                CancellationToken token
            ) =>
            {
                var (deviceInfo, country) = Extensions.Helpers.GetDeviceInfo(request);
                var result = await fido2Service.RegisterComplete(payload, deviceInfo, country);

                // Avoid serializing the certificate
                return Ok(result);
            })
            .WithParameterValidation()
            .RequireAuthorization(ApiKeyScopes.PublicRegister)
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));
    }
}