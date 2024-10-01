using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi.Filters;

namespace Passwordless.Api.OpenApi;

public static class OpenApiBootstrap
{
    public static void AddOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen(swagger =>
        {
            swagger.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Passwordless.Api.xml"), true);
            swagger.DocInclusionPredicate((docName, apiDesc) =>
            {
                var policy = (AuthorizationPolicy?)apiDesc.ActionDescriptor.EndpointMetadata.FirstOrDefault(x => x.GetType() == typeof(AuthorizationPolicy));
                if (policy == null)
                {
                    return false;
                }
                return !policy.AuthenticationSchemes.Contains(Constants.ManagementKeyAuthenticationScheme);
            });
            swagger.OperationFilter<AuthorizationOperationFilter>();
            swagger.OperationFilter<ExtendedStatusDescriptionsOperationFilter>();
            swagger.OperationFilter<ExternalDocsOperationFilter>();

            swagger.AddSecurityDefinition(Constants.PublicKeyAuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "Front-end integrations",
                Type = SecuritySchemeType.ApiKey,
                Name = Constants.PublicKeyHeaderName,
                Scheme = Constants.PublicKeyAuthenticationScheme,
                In = ParameterLocation.Header
            });

            swagger.AddSecurityDefinition(Constants.SecretKeyAuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "Back-end integrations",
                Type = SecuritySchemeType.ApiKey,
                Name = Constants.SecretKeyHeaderName,
                Scheme = Constants.SecretKeyAuthenticationScheme,
                In = ParameterLocation.Header
            });

            swagger.SupportNonNullableReferenceTypes();
            swagger.SwaggerDoc("v4", new OpenApiInfo
            {
                Version = "v4",
                Title = "Passwordless.dev API",
                TermsOfService = new Uri("https://bitwarden.com/terms/"),
                Contact = new OpenApiContact
                {
                    Email = "support@passwordless.dev",
                    Name = "Support",
                    Url = new Uri("https://bitwarden.com/contact/")
                }
            });
            swagger.SwaggerGeneratorOptions.IgnoreObsoleteActions = true;
        });
    }

    public static void UseOpenApi(this IApplicationBuilder app)
    {
        app.UseSwagger(c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            httpReq.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        }));

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v4/swagger.json", "v4");
            c.ConfigObject.ShowExtensions = true;
            c.ConfigObject.ShowCommonExtensions = true;
            c.DefaultModelsExpandDepth(-1);
            c.IndexStream = () => typeof(Program).Assembly.GetManifestResourceStream("Passwordless.Api.OpenApi.swagger.html");
            c.InjectStylesheet("/openapi.css");
            c.SupportedSubmitMethods();
        });
    }
}