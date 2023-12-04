using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;

namespace Passwordless.Api.Endpoints;

public static class AliasEndpoints
{
    public static void MapAliasEndpoints(this WebApplication app)
    {
        app.MapPost("/alias", async (AliasPayload payload,
                IFido2Service fido2Service,
                IEventLogger eventLogger) =>
            {
                await fido2Service.SetAlias(payload);

                eventLogger.LogUserAliasSetEvent(payload.UserId);

                return Results.NoContent();
            })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapGet("/alias/list", async (string userId, IFido2Service fido2Service) =>
        {
            // if payload is empty, throw exception
            if (string.IsNullOrEmpty(userId))
            {
                throw new ApiException("UserId is empty", 400);
            }

            var aliases = await fido2Service.GetAliases(userId);

            var res = ListResponse.Create(aliases);
            return Results.Ok(res);
        })
            .RequireSecretKey()
            .RequireCors("default");
    }
}