using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.AdminConsole.Endpoints;

public static class ApplicationEndpoints
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Applications");

        // The Blazor SSR sample uses the same solution for signing out, but we do not want to use all the endpoints.
        group.MapGet("/{appId}/Available", IsAppIdAvailableAsync).RequireAuthorization();

        group.MapGet("/{appId}/DownloadOnboarding", DownloadOnboardingAsync)
            .RequireAuthorization(CustomPolicy.HasAppRole);

        return endpoints;
    }

    public static async Task<IResult> IsAppIdAvailableAsync(
        [AsParameters] GetAppIdAvailabilityRequest request,
        [FromServices] IPasswordlessManagementClient client)
    {
        try
        {
            var result = await client.IsApplicationIdAvailableAsync(request);
            return Ok(result);
        }
        catch (PasswordlessApiException e)
        {
            return Json(
                e.Details,
                contentType: "application/problem+json",
                statusCode: e.Details.Status);
        }
    }

    public static async Task<IResult> DownloadOnboardingAsync(
        [FromServices] IApplicationService applicationService,
        [FromServices] IOptionsSnapshot<PasswordlessManagementOptions> options,
        [FromRoute] string appId)
    {
        var onboarding = await applicationService.GetOnboardingAsync(appId);

        if (onboarding == null)
        {
            return NotFound();
        }

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine($"API Url: {options.Value.ApiUrl}");
        contentBuilder.AppendLine($"Public ApiKey: {onboarding.ApiKey}");
        contentBuilder.AppendLine($"Secret ApiKey: {onboarding.ApiSecret}");

        var byteArray = Encoding.UTF8.GetBytes(contentBuilder.ToString());
        return File(byteArray, "application/octet-stream", $"passwordless-onboarding-{appId}.txt");
    }
}