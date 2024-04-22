using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Models.Apps;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

public static class AuthenticationConfigurationEndpoints
{
    public static void MapAuthenticationConfigurationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth-configs")
            .RequireCors("default")
            .WithOpenApi()
            .WithTags(OpenApiTags.AuthConfigs);

        group.MapGet("/list", async (
                [AsParameters] GetAuthenticationConfigurationsFilter request,
                [FromServices] IAuthenticationConfigurationService service) =>
            {
                var configurations = await service.GetAuthenticationConfigurationsAsync(request);

                return Ok(new GetAuthenticationConfigurationsResult
                {
                    Configurations = configurations
                        .Select(dto => dto.ToResponse())
                });
            })
            .WithSummary("A list of authentication scope configurations for your application. This will include the two default scopes of SignIn and StepUp.")
            .Produces<GetAuthenticationConfigurationsResult>()
            .RequireSecretKey();

        group.MapPost("/add", async (
                [FromBody] SetAuthenticationConfigurationRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService) =>
            {
                await authenticationConfigurationService.CreateAuthenticationConfigurationAsync(request);

                return Created();
            })
            .WithSummary("Creates or updates an authentication configuration for the sign-in process. In order to use this, it will have to be provided to the `stepup` client method via the purpose field")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapPost("/", async (
                [FromBody] SetAuthenticationConfigurationRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService,
                [FromServices] IEventLogger eventLogger) =>
            {
                await authenticationConfigurationService.UpdateAuthenticationConfigurationAsync(request);

                return NoContent();
            })
            .WithSummary("Creates or updates an authentication configuration for the sign-in process. In order to use this, it will have to be provided to the `stepup` client method via the purpose field")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapPost("/delete", async (
                [FromBody] DeleteAuthenticationConfigurationRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService,
                [FromServices] IEventLogger eventLogger) =>
            {
                var configuration = await authenticationConfigurationService.GetAuthenticationConfigurationAsync(request.Purpose);

                if (configuration == null) return NotFound();

                await authenticationConfigurationService.DeleteAuthenticationConfigurationAsync(configuration);

                eventLogger.LogAuthenticationConfigurationDeleted(configuration, request.PerformedBy);

                return NoContent();
            })
            .WithSummary("Deletes an authentication configuration for the provided purpose.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithParameterValidation()
            .RequireSecretKey();
    }
}