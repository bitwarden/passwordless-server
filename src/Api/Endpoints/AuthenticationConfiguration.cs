using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Api.OpenApi;
using Passwordless.Common.Models.Apps;
using Passwordless.Service;
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

        group.MapGet("/", async (
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
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService,
                HttpRequest httpRequest) =>
            {
                await authenticationConfigurationService.CreateAuthenticationConfigurationAsync(new AuthenticationConfigurationDto
                {
                    Purpose = new SignInPurpose(request.Purpose),
                    UserVerificationRequirement = request.UserVerificationRequirement,
                    TimeToLive = request.TimeToLive,
                    Tenant = httpRequest.GetTenantName()!
                });

                return Created();
            })
            .WithSummary(
                "Creates or updates an authentication configuration for the sign-in process. In order to use this, it will have to be provided to the `stepup` client method via the purpose field")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapPost("/", async (
                [FromBody] SetAuthenticationConfigurationRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService,
                HttpRequest httpRequest) =>
            {
                await authenticationConfigurationService.UpdateAuthenticationConfigurationAsync(new AuthenticationConfigurationDto
                {
                    Purpose = new SignInPurpose(request.Purpose),
                    UserVerificationRequirement = request.UserVerificationRequirement,
                    TimeToLive = request.TimeToLive,
                    Tenant = httpRequest.GetTenantName()!
                });

                return Ok();
            })
            .WithSummary(
                "Creates or updates an authentication configuration for the sign-in process. In order to use this, it will have to be provided to the `stepup` client method via the purpose field")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithParameterValidation()
            .RequireSecretKey();

        group.MapDelete("/", async (
                [FromBody] DeleteAuthenticationConfigurationRequest request,
                [FromServices] IAuthenticationConfigurationService authenticationConfigurationService) =>
            {
                var configuration = await authenticationConfigurationService.GetAuthenticationConfigurationAsync(request.Purpose);

                if (configuration == null) return NotFound();

                await authenticationConfigurationService.DeleteAuthenticationConfigurationAsync(configuration);

                return Ok();
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithParameterValidation()
            .RequireSecretKey();
    }
}