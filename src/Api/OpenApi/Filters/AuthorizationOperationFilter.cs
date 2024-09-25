using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Passwordless.Api.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Passwordless.Api.OpenApi.Filters;

public class AuthorizationOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var policy = (AuthorizationPolicy?)context.ApiDescription.ActionDescriptor.EndpointMetadata.SingleOrDefault(x => x.GetType() == typeof(AuthorizationPolicy));
        if (policy == null)
        {
            return;
        }

        switch (policy.AuthenticationSchemes.SingleOrDefault())
        {
            case Constants.PublicKeyAuthenticationScheme:
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = Constants.PublicKeyAuthenticationScheme
                            }
                        },
                        []
                    }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.PublicKeyHeaderName,
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Description = "Your public API key",
                        Example = new OpenApiString("yourappid:public:00000000000000000000000000000000"),
                        Nullable = false,
                        Type = "string"
                    }
                });
                break;
            case Constants.SecretKeyAuthenticationScheme:
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = Constants.SecretKeyAuthenticationScheme
                            }
                        },
                        []
                    }
                });
                break;
        }
    }
}