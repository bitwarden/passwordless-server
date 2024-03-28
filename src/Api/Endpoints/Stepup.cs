using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Api.OpenApi;
using Passwordless.Service;
using Passwordless.Service.Models;

namespace Passwordless.Api.Endpoints;

/// <summary>
/// Adds the step up endpoints
/// </summary>
public static class StepUpEndpoints
{
    public static void MapStepupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/stepup")
            .RequireCors("default")
            .WithTags(OpenApiTags.Stepup);

        group.MapPost("/", StepUpAsync).WithParameterValidation().RequirePublicKey();
        group.MapPost("/verify", StepUpVerifyAsync).WithParameterValidation().RequireSecretKey();
    }

    private static async Task<IResult> StepUpAsync(
        StepUpTokenRequest request,
        HttpRequest httpRequest,
        IFido2Service fido2Service)
    {
        var (device, country) = httpRequest.GetDeviceInfo();

        var response = await fido2Service.StepUpCompleteAsync(request, device, country);

        return Results.Ok(response);
    }

    private static async Task<IResult> StepUpVerifyAsync(
        StepUpVerifyRequest request,
        IFido2Service fido2Service)
    {
        var response = await fido2Service.StepUpVerifyAsync(request);

        return Results.Ok(response);
    }

}