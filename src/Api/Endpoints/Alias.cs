using Microsoft.AspNetCore.Authentication;
using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Api.Models;
using Passwordless.Common.Models;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using static Passwordless.Service.EventLog.EventFunctions;

namespace Passwordless.Api.Endpoints;

public static class AliasEndpoints
{
    public static void MapAliasEndpoints(this WebApplication app)
    {
        app.MapPost("/alias", async (AliasPayload payload,
                IFido2ServiceFactory fido2ServiceFactory,
                IEventLogger eventLogger,
                HttpRequest request,
                ISystemClock clock) =>
            {
                var fido2Service = await fido2ServiceFactory.CreateAsync();
                await fido2Service.SetAlias(payload);

                eventLogger.LogEvent(UserAliasSetEvent(payload.UserId, request.GetTenantName(), clock.UtcNow.UtcDateTime, new ApplicationSecretKey(request.GetApiSecret())));

                return Results.NoContent();
            })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapGet("/alias/list", async (string userId, IFido2ServiceFactory fido2ServiceFactory) =>
        {
            // if payload is empty, throw exception
            if (string.IsNullOrEmpty(userId))
            {
                throw new ApiException("UserId is empty", 400);
            }

            var fido2Service = await fido2ServiceFactory.CreateAsync();
            var aliases = await fido2Service.GetAliases(userId);

            var res = ListResponse.Create(aliases);
            return Results.Ok(res);
        })
            .RequireSecretKey()
            .RequireCors("default");
    }
}