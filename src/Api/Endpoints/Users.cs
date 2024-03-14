using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users")
            .RequireSecretKey();

        group.MapMethods("/list", new[] { "get" }, async (UserCredentialsService userService) =>
        {
            //userId = req.Query["userId"];
            // todo: Add Include credentials
            // todo: Add Include Aliases

            var result = await userService.GetAllUsers(null);
            var response = ListResponse.Create(result);
            return Ok(response);
        })
            .RequireCors("default");

        group.MapMethods("/count", new[] { "get" }, async (HttpRequest req, UserCredentialsService userService) =>
        {
            var res = await userService.GetUsersCount();

            return Ok(new CoundRecord(res));
        });

        group.MapPost("/delete", async (UserDeletePayload payload, UserCredentialsService userService, IEventLogger eventLogger) =>
        {
            await userService.DeleteUser(payload.UserId);

            eventLogger.LogDeletedUserEvent(payload.UserId);

            return Ok();
        });
    }

    public record CoundRecord(int Count);

    public record UserDeletePayload(string UserId);
}