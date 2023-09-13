using ApiHelpers;
using Microsoft.AspNetCore.Authentication;
using Passwordless.Api.Authorization;
using Passwordless.Common.Models;
using Passwordless.Service;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;
using static Passwordless.Service.AuditLog.Mappings.AuditEventExtensions;

namespace Passwordless.Server.Endpoints;

public static class RegisterEndpoints
{
    public static void MapRegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/register/token", async (RegisterToken registerToken,
                IFido2ServiceFactory fido2ServiceFactory,
                AuditLoggerProvider provider,
                HttpRequest request,
                ISystemClock clock) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var result = await fido2Service.CreateToken(registerToken);

            var logger = await provider.Create();
            logger.LogEvent(registerToken.ToEvent(request.GetTenantName(), clock.UtcNow.UtcDateTime, new PrivateKey(request.GetApiSecret())));

            return Ok(new RegisterTokenResponse(result));
        })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapPost("/register/begin", async (FidoRegistrationBeginDTO payload,
                IFido2ServiceFactory fido2ServiceFactory,
                HttpRequest request,
                AuditLoggerProvider provider,
                ISystemClock clock) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var result = await fido2Service.RegisterBegin(payload);

            var logger = await provider.Create();
            logger.LogEvent(payload.ToEvent(request.GetTenantName(), clock.UtcNow.UtcDateTime, new PublicKey(request.GetPublicApiKey())));

            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true));

        app.MapPost("/register/complete", async (RegistrationCompleteDTO payload,
                HttpRequest request,
                IFido2ServiceFactory fido2ServiceFactory,
                AuditLoggerProvider provider,
                ISystemClock clock) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var (deviceInfo, country) = Helpers.GetDeviceInfo(request);
            var result = await fido2Service.RegisterComplete(payload, deviceInfo, country);

            var logger = await provider.Create();
            logger.LogEvent(RegistrationCompletedEvent(result.Token, request.GetTenantName(), clock.UtcNow.UtcDateTime, new PublicKey(request.GetPublicApiKey())));

            // Avoid serializing the certificate
            return Ok(result);
        })
            .WithParameterValidation()
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true));
    }

    public record RegisterTokenResponse(string Token);
}