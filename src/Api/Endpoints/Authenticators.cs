using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service;
using Passwordless.Service.Features;
using Passwordless.Service.Helpers;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class AuthenticatorsEndpoints
{
    public static void MapAuthenticatorsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/authenticators")
            .RequireCors("default");

        group.MapGet("/list", ListConfiguredAuthenticatorsAsync)
            .RequireSecretKey();

        group.MapPost("/whitelist", WhitelistAuthenticatorsAsync)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapPost("/delist", DelistAuthenticatorsAsync)
            .WithParameterValidation()
            .RequireSecretKey();
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
        IFeatureContextProvider featureContextProvider)
    {
        var features = await featureContextProvider.UseContext();
        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation is not supported on your plan.", 403);
        }

        await service.WhitelistAuthenticatorsAsync(request);
        return NoContent();
    }

    public static async Task<IResult> DelistAuthenticatorsAsync(
        [FromBody] DelistAuthenticatorsRequest request,
        IApplicationService service,
        IFeatureContextProvider featureContextProvider)
    {
        var features = await featureContextProvider.UseContext();
        if (!features.AllowAttestation)
        {
            throw new ApiException("attestation_not_supported_on_plan", "Attestation is not supported on your plan.", 403);
        }

        await service.DelistAuthenticatorsAsync(request);
        return NoContent();
    }
}