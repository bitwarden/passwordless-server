using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Passwordless.Api.OpenApi.Filters;

/// <summary>
/// Links an operation to external documentation.
/// </summary>
public class ExternalDocsOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var externalDocs = context.MethodInfo.GetCustomAttribute<ExternalDocsAttribute>();
        if (externalDocs is not null)
        {
            operation.ExternalDocs = new OpenApiExternalDocs
            {
                Description = "External Documentation",
                Url = new Uri(externalDocs.Url)
            };
        }
    }
}