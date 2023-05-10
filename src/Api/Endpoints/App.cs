using Microsoft.AspNetCore.Http.Extensions;
using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service;
using static Microsoft.AspNetCore.Http.Results;

namespace Passwordless.Server.Endpoints;

public static class AccountEndpoints
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

        app.MapPost("/apps/delete", async (AppIdDTO payload, HttpRequest req, ISharedManagementService service) =>
            {
                var urib = new UriBuilder(req.GetDisplayUrl());
                urib.Path = "";
                var cancelLink = $"{urib.Uri.AbsoluteUri}apps/delete/cancel/{payload.AppId}";
                AppDeletionResult res = await service.DeleteAccount(payload.AppId, cancelLink);
                // logg this
                app.Logger.LogWarning("account/delete was issued {@Res}", res);
                return Ok(res);
            })
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

    public record AvailableResponse(bool Available);
    public record CancelResult(string Message);
}