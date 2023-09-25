using ApiHelpers;
using Passwordless.Api.Authorization;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.Features;

namespace Passwordless.Server.Endpoints;

public static class AuditLog
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        app.MapGet("events", GetAuditLogEvents)
            .RequireSecretKey()
            .RequireCors("default");
    }

    private static async Task<IResult> GetAuditLogEvents(int pageNumber,
        int numberOfResults,
        HttpRequest request,
        IAuditLogStorage storage,
        IFeatureContextProvider provider,
        CancellationToken cancellationToken)
    {
        if (!(await provider.UseContext()).AuditLoggingIsEnabled) return Results.Unauthorized();

        var tenantId = request.GetTenantName();

        var eventsTask = storage.GetAuditLogAsync(pageNumber, numberOfResults, cancellationToken);
        var eventCountTasks = storage.GetAuditLogCountAsync(cancellationToken);

        await Task.WhenAll(eventsTask, eventCountTasks);

        return Results.Ok(new
        {
            TenantId = tenantId,
            Events = eventsTask.Result.Select(x => x.ToEvent()),
            TotalEventCount = eventCountTasks.Result
        });
    }
}