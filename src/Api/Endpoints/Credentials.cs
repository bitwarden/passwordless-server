using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Models.Credentials;
using Passwordless.Service;
using Passwordless.Service.Helpers;
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

        group.MapGet("/list", ListGetCredentialsAsync);

        group.MapPost("/list", ListPostCredentialsAsync)
            .WithParameterValidation();
    }

    /// <summary>
    /// Deletes a credential.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="userCredentialsService"></param>
    /// <returns></returns>
    public static async Task<IResult> DeleteCredentialAsync(
        [FromBody] CredentialsDeleteDTO payload,
        UserCredentialsService userCredentialsService)
    {
        await userCredentialsService.DeleteCredentialAsync(payload.CredentialId);

        return NoContent();
    }

    /// <summary>
    /// Lists credentials for a given user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userCredentialService"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static async Task<IResult> ListGetCredentialsAsync(
        [FromQuery] string userId,
        [FromServices] UserCredentialsService userCredentialService)
    {
        if (userId == null)
        {
            throw new ApiException("Please supply UserId in the query string value", 400);
        }

        var result = await userCredentialService.GetAllCredentialsAsync(userId);

        var res = ListResponse.Create(result);

        return Ok(res);
    }

    /// <summary>
    /// Lists credentials for a given user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static Task<IResult> ListPostCredentialsAsync(
        [FromBody] GetCredentialsRequest request,
        [FromServices] UserCredentialsService service)
    {
        return ListGetCredentialsAsync(request.UserId, service);
    }
}