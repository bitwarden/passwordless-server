using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Passwordless.Api.OpenApi.Filters;

/// <summary>
/// Adds extended status descriptions to operations.
/// </summary>
public class ExtendedStatusDescriptionsOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var response in operation.Responses)
        {
            // https://docs.passwordless.dev/guide/api.html#status-codes
            switch (response.Key)
            {
                case "200":
                {
                    response.Value.Description = "\u2705 Everything is OK.";
                    break;
                }
                case "201":
                {
                    response.Value.Description = "\u2705 Everything is OK, resource created.";
                    break;
                }
                case "204":
                {
                    response.Value.Description = "\u2705 Everything is OK, response is empty.";
                    break;
                }
                case "400":
                {
                    response.Value.Description = "\ud83d\udd34 Bad request, see problem details for more info.";
                    break;
                }
                case "401":
                {
                    response.Value.Description = "\ud83d\udd34 You did not identify yourself.";
                    break;
                }
                case "403":
                {
                    response.Value.Description = "\ud83d\udd34 You are not allowed to perform the action, see problem details for more info.";
                    break;
                }
                case "409":
                {
                    response.Value.Description = "\ud83d\udd34 Conflict, see problem details for more info.";
                    break;
                }
                case "429":
                {
                    response.Value.Description = "\ud83d\udd34 Too many requests, see problem details for more info.";
                    break;
                }
                case "500":
                {
                    response.Value.Description = "\ud83d\udd34 Something went wrong on our side.";
                    break;
                }
            }
        }
    }
}