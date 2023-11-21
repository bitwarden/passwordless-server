using System.ComponentModel.DataAnnotations;
using MiniValidation;
using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Mappings;
using Passwordless.Service.Features;

namespace Passwordless.Api.Endpoints;

public static class EventLog
{
    public static void MapEventLogEndpoints(this WebApplication app)
    {
        app.MapGet("events", GetEventLogEvents)
            .RequireSecretKey()
            .RequireCors("default");
    }

    private static async Task<IResult> GetEventLogEvents(
        HttpRequest request,
        IEventLogStorage storage,
        IFeatureContextProvider provider,
        [AsParameters] GetEventLogEventsRequest getEventLogEventsRequest,
        CancellationToken cancellationToken)
    {
        if (!(await provider.UseContext()).EventLoggingIsEnabled) return Results.Unauthorized();

        if (!MiniValidator.TryValidate(getEventLogEventsRequest, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var tenantId = request.GetTenantNameFromKey();

        var events = await storage.GetEventLogAsync(getEventLogEventsRequest.PageNumber, getEventLogEventsRequest.NumberOfResults ?? 100, cancellationToken);

        return Results.Ok(new
        {
            TenantId = tenantId,
            Events = events.Select(x => x.ToEvent()),
            TotalEventCount = await storage.GetEventLogCountAsync(cancellationToken)
        });
    }

    public struct GetEventLogEventsRequest
    {
        public int PageNumber { get; set; }
        [Range(1, 1000)]
        public int? NumberOfResults { get; set; }
    }
}