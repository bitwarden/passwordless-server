using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi;
using Passwordless.Api.OpenApi.Extensions;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class AuthenticatorsEndpoints
{
    public static void MapAuthenticatorsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/authenticators")
            .RequireCors("default")
            .RequireSecretKey()
            .WithTags(OpenApiTags.Authenticators);

        group.MapGet("/list", ListConfiguredAuthenticatorsAsync)
            .WithOpenApi(o =>
            {
                o.Parameters.Get(nameof(ConfiguredAuthenticatorRequest.IsAllowed)).Description = "When 'true', all authenticators on the allowlist are returned. When 'false', all authenticators on the blocklist are returned.";
                return o;
            });

        group.MapPost("/add", AddAuthenticatorsAsync)
            .WithParameterValidation();

        group.MapPost("/remove", RemoveAuthenticatorsAsync)
            .WithParameterValidation();
    }

    /// <summary>
    /// List configured authenticators on the allowlist or blocklist. When the list is empty, all authenticators are allowed. (Requires the `Enterprise` plan.)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="service"></param>
    /// <param name="featureContextProvider"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static async Task<IResult> ListConfiguredAuthenticatorsAsync(
        [AsParameters] ConfiguredAuthenticatorRequest request,
        IApplicationService service,
        IFeatureContextProvider featureContextProvider)
    {
        var features = await featureContextProvider.UseContext();
        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation is not supported on your plan.", 403);
        }
        var result = await service.ListConfiguredAuthenticatorsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Adds authenticators to the allowlist or blocklist for use with attestation. (Requires the `Enterprise` plan.)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="service"></param>
    /// <param name="featureContextProvider"></param>
    /// <param name="eventLogger"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static async Task<IResult> AddAuthenticatorsAsync(
        [FromBody] AddAuthenticatorsRequest request,
        IApplicationService service,
        IFeatureContextProvider featureContextProvider,
        IEventLogger eventLogger)
    {
        var features = await featureContextProvider.UseContext();
        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation is not supported on your plan.", 403);
        }

        await service.AddAuthenticatorsAsync(request);
        eventLogger.LogAuthenticatorsAllowlistedEvent();
        return NoContent();
    }

    /// <summary>
    /// Removes authenticators from the allowlist or blocklist. (Requires the `Enterprise` plan.)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="service"></param>
    /// <param name="featureContextProvider"></param>
    /// <param name="eventLogger"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public static async Task<IResult> RemoveAuthenticatorsAsync(
        [FromBody] RemoveAuthenticatorsRequest request,
        IApplicationService service,
        IFeatureContextProvider featureContextProvider,
        IEventLogger eventLogger)
    {
        var features = await featureContextProvider.UseContext();
        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation is not supported on your plan.", 403);
        }

        await service.RemoveAuthenticatorsAsync(request);
        eventLogger.LogAuthenticatorsDelistedEvent();
        return NoContent();
    }
}