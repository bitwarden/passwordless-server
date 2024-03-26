using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Models.Credentials;
using Passwordless.Service;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class CredentialsEndpoints
{
    public static void MapCredentialsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/credentials")
            .RequireCors("default")
            .RequireSecretKey()
            .WithTags(OpenApiTags.Credentials);

        group.MapPost("/delete", DeleteCredentialAsync);

        group.MapGet("/list", ListGetCredentialsAsync)
            .WithParameterValidation();

        group.MapPost("/list", ListPostCredentialsAsync)
            .WithParameterValidation();
    }

    /// <summary>
    /// Deletes a credential.
    /// </summary>
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static async Task<IResult> DeleteCredentialAsync(
        [FromBody] CredentialsDeleteDTO payload,
        [FromServices] UserCredentialsService userCredentialsService)
    {
        await userCredentialsService.DeleteCredentialAsync(payload.CredentialId);

        return NoContent();
    }

    /// <summary>
    /// Lists credentials for a given user.
    /// </summary>
    [ProducesResponseType(typeof(ListResponse<StoredCredential>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static Task<IResult> ListGetCredentialsAsync(
        [AsParameters] GetCredentialsRequest request,
        [FromServices] UserCredentialsService service)
    {
        return ListCredentialsAsync(request, service);
    }

    /// <summary>
    /// Lists credentials for a given user.
    /// </summary>
    [ProducesResponseType(typeof(ListResponse<StoredCredential>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public static Task<IResult> ListPostCredentialsAsync(
        [FromBody] GetCredentialsRequest request,
        [FromServices] UserCredentialsService service)
    {
        return ListCredentialsAsync(request, service);
    }

    private static async Task<IResult> ListCredentialsAsync(
        GetCredentialsRequest request,
        UserCredentialsService service)
    {
        var result = await service.GetAllCredentialsAsync(request.UserId);
        var res = ListResponse.Create(result);
        return Ok(res);
    }
}