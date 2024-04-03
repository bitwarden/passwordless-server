using Microsoft.OpenApi.Models;

namespace Passwordless.Api.OpenApi.Extensions;

public static class OpenApiParameterListExtensions
{
    public static OpenApiParameter Get(this IList<OpenApiParameter> parameters, string name)
    {
        return parameters.Single(x => x.Name == name);
    }
}