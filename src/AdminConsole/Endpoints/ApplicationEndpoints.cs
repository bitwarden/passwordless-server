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
        group.MapGet("/{appId}/Available", async ([AsParameters] GetAppIdAvailabilityRequest request, IPasswordlessManagementClient client) =>
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
        }).RequireAuthorization();
        return endpoints;
    }
}