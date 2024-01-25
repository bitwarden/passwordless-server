using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
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
            .RequireSecretKey();

        group.MapGet("/list", ListConfiguredAuthenticatorsAsync);

        group.MapPost("/whitelist", WhitelistAuthenticatorsAsync)
            .WithParameterValidation();

        group.MapPost("/delist", DelistAuthenticatorsAsync)
            .WithParameterValidation();
    }

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

    public static async Task<IResult> WhitelistAuthenticatorsAsync(
        [FromBody] WhitelistAuthenticatorsRequest request,
        IApplicationService service,
        IFeatureContextProvider featureContextProvider,
        IEventLogger eventLogger)
    {
        var features = await featureContextProvider.UseContext();
        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation is not supported on your plan.", 403);
        }

        await service.WhitelistAuthenticatorsAsync(request);
        eventLogger.LogAuthenticatorsWhitelistedEvent();
        return NoContent();
    }

    public static async Task<IResult> DelistAuthenticatorsAsync(
        [FromBody] DelistAuthenticatorsRequest request,
        IApplicationService service,
        IFeatureContextProvider featureContextProvider,
        IEventLogger eventLogger)
    {
        var features = await featureContextProvider.UseContext();
        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation is not supported on your plan.", 403);
        }

        await service.DelistAuthenticatorsAsync(request);
        eventLogger.LogAuthenticatorsDelistedEvent();
        return NoContent();
    }
}