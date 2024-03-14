using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Api.OpenApi;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;

namespace Passwordless.Api.Endpoints;

public static class AliasEndpoints
{
    public static void MapAliasEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/alias")
            .RequireCors("default")
            .RequireSecretKey()
            .WithTags(OpenApiTags.Aliases);

        group.MapPost("", async (AliasPayload payload,
                IFido2Service fido2Service,
                IEventLogger eventLogger) =>
            {
                await fido2Service.SetAliasAsync(payload);

                eventLogger.LogUserAliasSetEvent(payload.UserId);

                return Results.NoContent();
            })
            .WithParameterValidation();

        group.MapGet("/list", async (string userId, IFido2Service fido2Service) =>
        {
            // if payload is empty, throw exception
            if (string.IsNullOrEmpty(userId))
            {
                throw new ApiException("UserId is empty", 400);
            }

            var aliases = await fido2Service.GetAliases(userId);

            var res = ListResponse.Create(aliases);
            return Results.Ok(res);
        });
    }
}