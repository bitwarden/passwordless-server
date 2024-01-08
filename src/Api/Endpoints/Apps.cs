using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Passwordless.Api.Authorization;
using Passwordless.Api.Helpers;
using Passwordless.Api.Models;
using Passwordless.Common.Models.Apps;
using Passwordless.Service;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Extensions.Models;
using Passwordless.Service.Features;
using Passwordless.Service.Models;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Api.Endpoints;

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
                [FromBody] CreateAppDto payload,
                ISharedManagementService service,
                IEventLogger eventLogger) =>
            {
                var result = await service.GenerateAccount(appId, payload);

                eventLogger.LogApplicationCreatedEvent(payload.AdminEmail);

                return Ok(result);
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/freeze", async ([FromRoute] string appId,
                ISharedManagementService service,
                IEventLogger eventLogger) =>
            {
                await service.FreezeAccount(appId);

                eventLogger.LogAppFrozenEvent();

                return NoContent();
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/unfreeze", async ([FromRoute] string appId,
                ISharedManagementService service,
                IEventLogger eventLogger) =>
            {
                await service.UnFreezeAccount(appId);

                eventLogger.LogAppUnfrozenEvent();

                return NoContent();
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/public-keys", CreatePublicKeyAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/secret-keys", CreateSecretKeyAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapGet("/admin/apps/{appId}/api-keys", ListApiKeysAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/api-keys/{apiKeyId}/lock", LockApiKeyAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/admin/apps/{appId}/api-keys/{apiKeyId}/unlock", UnlockApiKeyAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapDelete("/admin/apps/{appId}/api-keys/{apiKeyId}", DeleteApiKeyAsync)
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
        app.MapPost("/admin/apps/{appId}/cancel-delete", CancelDeletionAsync)
            .RequireManagementKey()
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

        app.MapPost("admin/apps/{appId}/sign-in-generate-token-endpoint/enable", EnableGenerateSignInTokenEndpoint)
            .WithParameterValidation()
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("admin/apps/{appId}/sign-in-generate-token-endpoint/disable", DisableGenerateSignInTokenEndpoint)
            .WithParameterValidation()
            .RequireManagementKey()
            .RequireCors("default");
    }

    public static async Task<IResult> CreatePublicKeyAsync(
        [FromRoute] string appId,
        [FromBody] CreatePublicKeyRequest payload,
        ISharedManagementService service,
        IEventLogger eventLogger)
    {
        var result = await service.CreateApiKeyAsync(appId, payload);
        eventLogger.LogApiKeyCreatedEvent(result.ApiKey);
        return Ok(result);
    }

    public static async Task<IResult> CreateSecretKeyAsync(
        [FromRoute] string appId,
        [FromBody] CreateSecretKeyRequest payload,
        ISharedManagementService service,
        IEventLogger eventLogger)
    {
        var result = await service.CreateApiKeyAsync(appId, payload);
        eventLogger.LogApiKeyCreatedEvent(result.ApiKey);
        return Ok(result);
    }

    public static async Task<IResult> ListApiKeysAsync(
        [FromRoute] string appId,
        ISharedManagementService service,
        IEventLogger eventLogger)
    {
        var apiKeys = await service.ListApiKeysAsync(appId);
        eventLogger.LogApiKeysEnumeratedEvent();
        return Ok(apiKeys);
    }

    public static async Task<IResult> LockApiKeyAsync(
        [FromRoute] string appId,
        [FromRoute] string apiKeyId,
        ISharedManagementService service,
        IEventLogger eventLogger)
    {
        await service.LockApiKeyAsync(appId, apiKeyId);
        eventLogger.LogApiKeyLockedEvent(apiKeyId);
        return NoContent();
    }

    public static async Task<IResult> UnlockApiKeyAsync(
        [FromRoute] string appId,
        [FromRoute] string apiKeyId,
        ISharedManagementService service,
        IEventLogger eventLogger)
    {
        await service.UnlockApiKeyAsync(appId, apiKeyId);
        eventLogger.LogApiKeyUnlockedEvent(apiKeyId);
        return NoContent();
    }

    public static async Task<IResult> DeleteApiKeyAsync(
        [FromRoute] string appId,
        [FromRoute] string apiKeyId,
        ISharedManagementService service,
        IEventLogger eventLogger)
    {
        await service.DeleteApiKeyAsync(appId, apiKeyId);
        eventLogger.LogApiKeyDeletedEvent(apiKeyId);
        return NoContent();
    }

    public static async Task<IResult> ManageFeaturesAsync(
        [FromRoute] string appId,
        [FromBody] ManageFeaturesRequest payload,
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

        var dto = new AppFeatureResponse(
            featuresContext.EventLoggingIsEnabled,
            featuresContext.EventLoggingRetentionPeriod,
            featuresContext.DeveloperLoggingEndsAt,
            featuresContext.MaxUsers,
            featuresContext.Attestation.ToDto(),
            featuresContext.IsGenerateSignInTokenEndpointEnabled);

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
        [FromBody] MarkDeleteApplicationRequest payload,
        ISharedManagementService service,
        IRequestContext requestContext,
        ILogger logger,
        IEventLogger eventLogger)
    {
        var baseUrl = requestContext.GetBaseUrl();
        var result = await service.MarkDeleteApplicationAsync(appId, payload.DeletedBy, baseUrl);
        logger.LogWarning("mark account/delete was issued {@Res}", result);

        eventLogger.LogAppMarkedToDeleteEvent(payload.DeletedBy);

        return Ok(result);
    }

    public static async Task<IResult> GetApplicationsPendingDeletionAsync(ISharedManagementService service)
    {
        var result = await service.GetApplicationsPendingDeletionAsync();
        return Ok(result);
    }

    public static async Task<IResult> CancelDeletionAsync(
        [FromRoute] string appId,
        ISharedManagementService service,
        IEventLogger eventLogger,
        ISystemClock clock)
    {
        await service.UnFreezeAccount(appId);
        var res = new CancelApplicationDeletionResponse("Your account will not be deleted since the process was aborted with the cancellation link");

        eventLogger.LogAppDeleteCancelledEvent();

        return Ok(res);
    }

    public static async Task<IResult> EnableGenerateSignInTokenEndpoint(
        [FromRoute] string appId,
        EnableGenerateSignInTokenEndpointRequest request,
        ISharedManagementService managementService,
        IEventLogger eventLogger)
    {
        await managementService.EnableGenerateSignInTokenEndpoint(appId);
        eventLogger.LogGenerateSignInTokenEndpointEnabled(request.PerformedBy);
        return NoContent();
    }

    public static async Task<IResult> DisableGenerateSignInTokenEndpoint(
        [FromRoute] string appId,
        DisableGenerateSignInTokenEndpointRequest request,
        ISharedManagementService managementService,
        IEventLogger eventLogger)
    {
        await managementService.DisableGenerateSignInTokenEndpoint(appId);
        eventLogger.LogGenerateSignInTokenEndpointDisabled(request.PerformedBy);
        return NoContent();
    }

    public record EnableGenerateSignInTokenEndpointRequest(string PerformedBy);

    public record DisableGenerateSignInTokenEndpointRequest(string PerformedBy);

    public record AvailableResponse(bool Available);

    public record CancelResult(string Message);
}