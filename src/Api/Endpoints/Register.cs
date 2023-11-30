using Passwordless.Api.Authorization;
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
                IFido2ServiceFactory fido2ServiceFactory,
                CancellationToken token
            ) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync(token);
                var result = await fido2Service.CreateRegisterToken(registerToken);

                return Ok(new RegisterTokenResponse(result));
            })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapPost("/register/begin", async (
                FidoRegistrationBeginDTO payload,
                IFido2ServiceFactory fido2ServiceFactory,
                CancellationToken token
            ) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync(token);
                var result = await fido2Service.RegisterBegin(payload);

                return Ok(result);
            })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/register/complete", async (
                RegistrationCompleteDTO payload,
                HttpRequest request,
                IFido2ServiceFactory fido2ServiceFactory,
                CancellationToken token
            ) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync(token);
                var (deviceInfo, country) = Extensions.Helpers.GetDeviceInfo(request);
                var result = await fido2Service.RegisterComplete(payload, deviceInfo, country);

                // Avoid serializing the certificate
                return Ok(result);
            })
            .WithParameterValidation()
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));
    }
}