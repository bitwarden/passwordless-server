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
            .WithParameterValidation()
            .WithTags(OpenApiTags.Stepup);

        group.MapPost("/", StepUpAsync).RequirePublicKey();
        group.MapPost("/verify", StepUpVerifyAsync).RequireSecretKey();
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

    private static async Task<IResult> StepUpVerifyAsync(StepUpVerifyRequest request)
    {
        // validate the token sent
        // return verified user

        return Results.Ok();
    }

}