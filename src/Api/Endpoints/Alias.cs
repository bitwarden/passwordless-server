using System.Security.Claims;
using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage;

namespace Passwordless.Server.Endpoints;

public static class AliasEndpoints
{
    public static void MapAliasEndpoints(this WebApplication app)
    {
        app.MapPost("/alias", async (AliasPayload payload, ClaimsPrincipal user, ITenantStorage storage) =>
        {
            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);
            await service.SetAlias(payload);

            return Results.Ok();
        })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapGet("/alias/list", async (string userId, ClaimsPrincipal user, ITenantStorage storage) =>
        {
            // if payload is empty, throw exception
            if (string.IsNullOrEmpty(userId))
            {
                throw new ApiException("UserId is empty", 400);
            }

            var accountName = user.GetAccountName();
            var service = await Fido2ServiceEndpoints.Create(accountName, app.Logger, app.Configuration, storage);
            var aliases = await service.GetAliases(userId);

            var res = ListResponse.Create(aliases);
            return Results.Ok(res);
        })
            .RequireSecretKey()
            .RequireCors("default");
    }
}