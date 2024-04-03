using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Api.OpenApi;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users")
            .RequireSecretKey()
            .WithTags(OpenApiTags.Users);

        group.MapGet("/list", GetUsersAsync)
            .RequireCors("default");

        group.MapGet("/count", GetUsersCountAsync);

        group.MapPost("/delete", DeleteUserAsync)
            .WithParameterValidation();
    }

    /// <summary>
    /// Get a list of users.
    /// </summary>
    [ProducesResponseType(typeof(ListResponse<UserSummary>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> GetUsersAsync(
        [FromServices] UserCredentialsService userService)
    {
        var result = await userService.GetAllUsers(null!);
        var response = ListResponse.Create(result);
        return Ok(response);
    }

    /// <summary>
    /// Get the amount of users.
    /// </summary>
    [ProducesResponseType(typeof(CountRecord), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> GetUsersCountAsync(
        [FromServices] UserCredentialsService userService)
    {
        var res = await userService.GetUsersCount();

        return Ok(new CountRecord(res));
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> DeleteUserAsync(
        [FromBody] UserDeletePayload payload,
        [FromServices] UserCredentialsService userService,
        [FromServices] IEventLogger eventLogger)
    {
        await userService.DeleteUser(payload.UserId);

        eventLogger.LogDeletedUserEvent(payload.UserId);

        return NoContent();
    }

    public record CountRecord(int Count);

    public record UserDeletePayload(
        [MinLength(1), Required]
        string UserId);
}