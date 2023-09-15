using ApiHelpers;
using Microsoft.AspNetCore.Authentication;
using Passwordless.Api.Authorization;
using Passwordless.Common.Models;
using Passwordless.Service;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;
using static Passwordless.Service.AuditLog.Mappings.AuditEventExtensions;

namespace Passwordless.Server.Endpoints;

public static class SigninEndpoints
{
    public static void MapSigninEndpoints(this WebApplication app)
    {

        app.MapPost("/signin/begin", async (SignInBeginDTO payload, IFido2ServiceFactory fido2ServiceFactory, HttpRequest request, AuditLoggerProvider provider, ISystemClock clock) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var result = await fido2Service.SignInBegin(payload);

            var logger = await provider.Create();
            logger.LogEvent(UserSignInBeganEvent(payload.UserId, clock.UtcNow.UtcDateTime, request.GetTenantName(), new PublicKey(request.GetPublicApiKey())));

            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/complete", async (SignInCompleteDTO payload, HttpRequest request, IFido2ServiceFactory fido2ServiceFactory, AuditLoggerProvider provider, ISystemClock clock) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var (deviceInfo, country) = Helpers.GetDeviceInfo(request);
            var result = await fido2Service.SignInComplete(payload, deviceInfo, country);

            var logger = await provider.Create();
            logger.LogEvent(UserSignInCompletedEvent("specific user", clock.UtcNow.UtcDateTime, request.GetTenantName(), new PublicKey(request.GetPublicApiKey())));

            return Ok(result);
        })
            .RequirePublicKey()
            .RequireCors("default")
            .WithMetadata(new HttpMethodMetadata(new string[] { "POST" }, acceptCorsPreflight: true)); ;

        app.MapPost("/signin/verify", async (SignInVerifyDTO payload, IFido2ServiceFactory fido2ServiceFactory, HttpRequest request, AuditLoggerProvider provider, ISystemClock clock) =>
        {
            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var result = await fido2Service.SignInVerify(payload);

            var logger = await provider.Create();
            logger.LogEvent(UserSignInTokenVerifiedEvent("i did it", clock.UtcNow.UtcDateTime, request.GetTenantName(), new PrivateKey(request.GetApiSecret())));

            return Ok(result);
        })
            .RequireSecretKey()
            .RequireCors("default");
    }
}