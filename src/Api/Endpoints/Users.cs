using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using static Microsoft.AspNetCore.Http.Results;
using static Passwordless.Service.EventLog.EventFunctions;

namespace Passwordless.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        app.MapMethods("/users/list", new[] { "get" }, async (UserCredentialsService userService) =>
        {
            //userId = req.Query["userId"];
            // todo: Add Include credentials
            // todo: Add Include Aliases

            var result = await userService.GetAllUsers(null);
            var response = ListResponse.Create(result);
            return Ok(response);
        })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapMethods("/users/count", new[] { "get" }, async (HttpRequest req, UserCredentialsService userService) =>
        {
            var res = await userService.GetUsersCount();

            return Ok(new CoundRecord(res));
        })
            .RequireSecretKey();

        app.MapPost("/users/delete", async (UserDeletePayload payload, UserCredentialsService userService, IEventLogger eventLogger) =>
        {
            await userService.DeleteUser(payload.UserId);

            eventLogger.LogEvent(DeletedUserEvent(payload.UserId));

            return Ok();
        })
            .RequireSecretKey();
    }

    public record CoundRecord(int Count);

    public record UserDeletePayload(string UserId);
}