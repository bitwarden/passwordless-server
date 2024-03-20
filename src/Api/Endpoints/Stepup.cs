using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
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
        ITokenService tokenService,
        TimeProvider timeProvider,
        IEventLogger eventLogger)
    {
        var token = await tokenService.EncodeTokenAsync(new StepUpToken
        {
            ExpiresAt = default,
            TokenId = default,
            Type = null,
            UserId = null,
            CreatedAt = default,
            RpId = null,
            Origin = null,
            Success = false,
            Device = null,
            Country = null,
            Context = request.Context.Context
        }, "verify_");


        eventLogger.LogStepUpTokenCreated(request);

        return Results.Ok(token);
    }

    private static async Task<IResult> StepUpVerifyAsync(StepUpValidateRequest request)
    {
        // validate the token sent
        // return verified user

        return Results.Ok();
    }

    private record StepUpValidateRequest(StepUpToken Token);
}