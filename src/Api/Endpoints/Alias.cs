using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Constants;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class AliasEndpoints
{
    public static void MapAliasEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/alias")
            .RequireCors("default")
            .RequireSecretKey()
            .WithTags(OpenApiTags.Aliases);

        group.MapPost("", SetAliasAsync)
            .WithParameterValidation();

        group.MapGet("/list", ListAliasesAsync);
    }

    /// <summary>
    /// Sets one or more aliases for an existing user and removes existing aliases that are not included in the request.
    /// </summary>
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public static async Task<IResult> SetAliasAsync(
        [FromBody] AliasPayload payload,
        IFido2Service fido2Service,
        IEventLogger eventLogger)
    {
        await fido2Service.SetAliasAsync(payload);

        eventLogger.LogUserAliasSetEvent(payload.UserId);

        return NoContent();
    }

    /// <summary>
    /// Lists all aliases for a given user.
    /// </summary>
    [ProducesResponseType(typeof(ListResponse<AliasPointer>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> ListAliasesAsync(
        [FromQuery] string userId,
        IFido2Service fido2Service)
    {
        // if payload is empty, throw exception
        if (string.IsNullOrEmpty(userId))
        {
            throw new ApiException("UserId is empty", 400);
        }

        var aliases = await fido2Service.GetAliases(userId);

        var res = ListResponse.Create(aliases);
        return Ok(res);
    }
}