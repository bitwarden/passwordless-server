using System.ComponentModel.DataAnnotations;
using ApiHelpers;
using MiniValidation;
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

    private static async Task<IResult> GetAuditLogEvents(
        HttpRequest request,
        IAuditLogStorage storage,
        IFeatureContextProvider provider,
        [AsParameters] GetAuditLogEventsRequest getAuditLogEventsRequest,
        CancellationToken cancellationToken)
    {
        if (!(await provider.UseContext()).AuditLoggingIsEnabled) return Results.Unauthorized();

        if (!MiniValidator.TryValidate(getAuditLogEventsRequest, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var tenantId = request.GetTenantName();

        var eventsTask = storage.GetAuditLogAsync(getAuditLogEventsRequest.PageNumber, getAuditLogEventsRequest.NumberOfResults ?? 100, cancellationToken);
        var eventCountTasks = storage.GetAuditLogCountAsync(cancellationToken);

        await Task.WhenAll(eventsTask, eventCountTasks);

        return Results.Ok(new
        {
            TenantId = tenantId,
            Events = eventsTask.Result.Select(x => x.ToEvent()),
            TotalEventCount = eventCountTasks.Result
        });
    }

    public struct GetAuditLogEventsRequest
    {
        public int PageNumber { get; set; }
        [Range(1, 1000)]
        public int? NumberOfResults { get; set; }
    }
}