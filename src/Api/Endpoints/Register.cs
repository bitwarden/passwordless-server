using System.Security.Claims;
using ApiHelpers;
using Passwordless.Api.Authorization;
using Passwordless.Service;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Server.Endpoints;

public static class RegisterEndpoints
{

    public static void MapRegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/register/token", async (ClaimsPrincipal user, RegisterTokenDTO registerToken, ITenantStorage storage) =>
        {
            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);
            var result = await service.CreateToken(registerToken);

            return Ok(new RegisterToken(result));
        })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapPost("/register/begin", async (ClaimsPrincipal user, FidoRegistrationBeginDTO payload, ITenantStorage storage) =>
        {
            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);
            var result = await service.RegisterBegin(payload);
            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/register/complete", async (RegistrationCompleteDTO payload, HttpRequest request, ClaimsPrincipal user, ITenantStorage storage) =>
        {
            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);

            var (deviceInfo, country) = Helpers.GetDeviceInfo(request);
            var result = await service.RegisterComplete(payload, deviceInfo, country);

            // Avoid serializing the certificate
            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true));
    }

    public record RegisterToken(string Token);
}