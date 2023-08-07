using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service;
using static Microsoft.AspNetCore.Http.Results;

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

        app.MapPost("/apps/create", async (AppCreateDTO payload, ISharedManagementService service) =>
            {
                var result = await service.GenerateAccount(payload.AppId, payload.AdminEmail);

                return Ok(result);
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/apps/freeze", async (AppIdDTO payload, ISharedManagementService service) =>
            {
                await service.FreezeAccount(payload.AppId);
                return Ok();
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/apps/unfreeze", async (AppIdDTO payload, ISharedManagementService service) =>
            {
                await service.UnFreezeAccount(payload.AppId);
                return Ok();
            })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapGet("/apps/list-pending-deletion", GetApplicationsPendingDeletionAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/apps/delete", DeleteApplicationAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("/apps/mark-delete", MarkDeleteApplicationAsync)
            .RequireManagementKey()
            .RequireCors("default");

        app.MapGet("/apps/delete/cancel/{appId}",
                async (string appId, HttpContext ctx, HttpRequest req, ISharedManagementService service) =>
                {
                    await service.UnFreezeAccount(appId);

                    var res = new CancelResult(
                        "Your account will not be deleted since the process was aborted with the cancellation link");
                    return Ok(res);
                })
            .RequireCors("default");
    }

    public static async Task<IResult> DeleteApplicationAsync(
        AppIdDTO payload,
        ISharedManagementService service,
        ILogger logger)
    {
        var result = await service.DeleteApplicationAsync(payload.AppId);
        logger.LogWarning("account/delete was issued {@Res}", result);
        return Ok(result);
    }

    public static async Task<IResult> MarkDeleteApplicationAsync(
        MarkDeleteAppDto payload,
        ISharedManagementService service,
        ILogger logger)
    {
        var result = await service.MarkDeleteApplicationAsync(payload.AppId, payload.DeletedBy);
        logger.LogWarning("mark account/delete was issued {@Res}", result);
        return Ok(result);
    }

    public static async Task<IResult> GetApplicationsPendingDeletionAsync(ISharedManagementService service)
    {
        var result = await service.GetApplicationsPendingDeletionAsync();
        return Ok(result);
    }

    public record AvailableResponse(bool Available);
    public record CancelResult(string Message);
}