using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Services;
using Passwordless.Common.Utils;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.AdminConsole.Endpoints;

public static class SetupEndpoints
{
    public static void MapSetupEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Setup");

        group.MapGet("/Status", GetStatusAsync);
    }

    public static async Task<IResult> GetStatusAsync(
        [FromServices] IOptions<PasswordlessOptions> options,
        [FromServices] IApplicationService applicationService,
        [FromServices] TimeProvider timeProvider)
    {
        var apiKey = options.Value.ApiKey;
        var apiSecret = options.Value.ApiSecret;

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            return Ok(new StatusResponse(false));
        }

        var publicAppId = ApiKeyUtils.GetAppId(apiKey);
        var secretAppId = ApiKeyUtils.GetAppId(apiSecret);

        if (publicAppId != secretAppId)
        {
            return Ok(new StatusResponse(false));
        }

        var onboarding = await applicationService.GetOnboardingAsync(publicAppId);

        // If onboarding doesn't exist, then likely the api keys don't have a matching application.
        if (onboarding == null)
        {
            return Ok(new StatusResponse(false));
        }

        // If ApiKey and ApiSecret matches, then setup is completed.
        if (onboarding.ApiKey == apiKey && onboarding.ApiSecret == apiSecret)
        {
            return Ok(new StatusResponse(true));
        }

        // If ApiKey and ApiSecret doesn't match, then onboarding could have expired.
        if (onboarding.SensitiveInfoExpireAt < timeProvider.GetUtcNow().DateTime)
        {
            return Ok(new StatusResponse(true));
        }

        return Ok(new StatusResponse(false));
    }

    public record StatusResponse(bool HasSetupCompleted);
}