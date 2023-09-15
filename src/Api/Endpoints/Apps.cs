using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Helpers;
using Passwordless.Api.Models;
using Passwordless.Service;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.Features;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;
using static Passwordless.Service.AuditLog.Mappings.AuditEventExtensions;

namespace Passwordless.Server.Endpoints;

public static class AppsEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/apps/available", async (AppAvailable payload, ISharedManagementService accountService) =>
            {
                if (payload.AppId.Length < 3)
                {
                    return Conflict(false);
                }

                var result = await accountService.IsAvailable(payload.AppId);

                var res = new AvailableResponse(result);

                return result ? Ok(res) : Conflict(res);
            })
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/create", async (
                [FromRoute] string appId,
                [FromBody] AppCreateDTO payload,
                ISharedManagementService service,
                AuditLoggerProvider provider,
                ISystemClock clock) =>
            {
                var result = await service.GenerateAccount(appId, payload);

                var logger = await provider.Create();
                logger.LogEvent(ApplicationCreatedEvent(payload.AdminEmail, appId, clock.UtcNow.UtcDateTime));

                return Ok(result);
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/freeze", async ([FromRoute] string appId,
                ISharedManagementService service,
                AuditLoggerProvider provider,
                ISystemClock clock) =>
            {
                await service.FreezeAccount(appId);

                var logger = await provider.Create();
                logger.LogEvent(AppFrozenEvent(appId, clock.UtcNow.UtcDateTime));

                return NoContent();
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/unfreeze", async ([FromRoute] string appId,
                ISharedManagementService service,
                AuditLoggerProvider provider,
                ISystemClock clock) =>
            {
                await service.UnFreezeAccount(appId);

                var logger = await provider.Create();
                logger.LogEvent(AppUnfrozenEvent(appId, clock.UtcNow.UtcDateTime));

                return NoContent();
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapGet("/apps/list-pending-deletion", GetApplicationsPendingDeletionAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapDelete("/admin/apps/{appId}", DeleteApplicationAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/mark-delete", MarkDeleteApplicationAsync)
            .RequireManagementKey()
            .RequireCors("default");

        // This will be used by an email link to cancel
        app.MapGet("/apps/delete/cancel/{appId}", CancelDeletionAsync)
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/features", ManageFeaturesAsync)
            .WithParameterValidation()
            .RequireManagementKey()
            .RequireCors("default");

        app.MapGet("/admin/apps/{appId}/features", GetFeaturesAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/apps/features", SetFeaturesAsync)
            .WithParameterValidation()
            .RequireSecretKey()
            .RequireCors("default");
    }

    public static async Task<IResult> ManageFeaturesAsync(
        [FromRoute] string appId,
        [FromBody] ManageFeaturesDto payload,
        ISharedManagementService service)
    {
        await service.SetFeaturesAsync(appId, payload);
        return NoContent();
    }

    public static async Task<IResult> SetFeaturesAsync(
        SetFeaturesDto payload,
        IApplicationService service)
    {
        await service.SetFeaturesAsync(payload);
        return NoContent();
    }

    public static async Task<IResult> GetFeaturesAsync(IFeatureContextProvider featuresContextProvider)
    {
        var featuresContext = await featuresContextProvider.UseContext();
        var dto = new AppFeatureDto
        {
            AuditLoggingIsEnabled = featuresContext.AuditLoggingIsEnabled,
            AuditLoggingRetentionPeriod = featuresContext.AuditLoggingRetentionPeriod,
            DeveloperLoggingEndsAt = featuresContext.DeveloperLoggingEndsAt
        };
        return Ok(dto);
    }

    public static async Task<IResult> DeleteApplicationAsync(
        [FromRoute] string appId,
        ISharedManagementService service,
        ILogger logger)
    {
        var result = await service.DeleteApplicationAsync(appId);
        logger.LogWarning("account/delete was issued {@Res}", result);
        return Ok(result);
    }

    public static async Task<IResult> MarkDeleteApplicationAsync(
        [FromRoute] string appId,
        [FromBody] MarkDeleteAppDto payload,
        ISharedManagementService service,
        IRequestContext requestContext,
        ILogger logger,
        AuditLoggerProvider provider,
        ISystemClock clock)
    {
        var baseUrl = requestContext.GetBaseUrl();
        var result = await service.MarkDeleteApplicationAsync(appId, payload.DeletedBy, baseUrl);
        logger.LogWarning("mark account/delete was issued {@Res}", result);

        var auditLogger = await provider.Create();
        auditLogger.LogEvent(AppMarkedToDeleteEvent(payload.DeletedBy, appId, clock.UtcNow.UtcDateTime));

        return Ok(result);
    }

    public static async Task<IResult> GetApplicationsPendingDeletionAsync(ISharedManagementService service)
    {
        var result = await service.GetApplicationsPendingDeletionAsync();
        return Ok(result);
    }

    public static async Task<IResult> CancelDeletionAsync(string appId,
        ISharedManagementService service,
        AuditLoggerProvider provider,
        ISystemClock clock)
    {
        await service.UnFreezeAccount(appId);
        var res = new CancelResult("Your account will not be deleted since the process was aborted with the cancellation link");

        var auditLogger = await provider.Create();
        auditLogger.LogEvent(AppDeleteCancelledEvent(appId, clock.UtcNow.UtcDateTime));

        return Ok(res);
    }

    public record AvailableResponse(bool Available);

    public record CancelResult(string Message);
}