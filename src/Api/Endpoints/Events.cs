using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using MiniValidation;
using Passwordless.Api.Authorization;
using Passwordless.Api.Extensions;
using Passwordless.Api.OpenApi;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Mappings;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Features;
using Passwordless.Service.Models;

namespace Passwordless.Api.Endpoints;

public static class EventLog
{
    public static void MapEventLogEndpoints(this WebApplication app)
    {
        app.MapGet("events", GetEventLogEventsAsync)
            .RequireSecretKey()
            .RequireCors("default")
            .WithTags(OpenApiTags.EventLogging)
            .WithParameterValidation();
    }

    /// <summary>
    /// Lists event logs. (Requires the `Enterprise` plan.)
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(GetEventLogEventsResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden, MediaTypeNames.Application.ProblemJson)]

    private static async Task<IResult> GetEventLogEventsAsync(
        HttpRequest request,
        IEventLogStorage storage,
        IFeatureContextProvider provider,
        [AsParameters] GetEventLogEventsRequest getEventLogEventsRequest,
        CancellationToken cancellationToken)
    {
        if (!(await provider.UseContext()).EventLoggingIsEnabled) return Results.Forbid();

        if (!MiniValidator.TryValidate(getEventLogEventsRequest, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var events = await storage.GetEventLogAsync(getEventLogEventsRequest.PageNumber, getEventLogEventsRequest.NumberOfResults ?? 100, cancellationToken);

        return Results.Ok(new GetEventLogEventsResponse
        {
            TenantId = request.GetTenantNameFromKey() ?? string.Empty,
            Events = events?.Select(x => x.ToEvent()) ?? new List<EventResponse>(),
            TotalEventCount = await storage.GetEventLogCountAsync(cancellationToken)
        });
    }

    private struct GetEventLogEventsRequest
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }

        [Range(1, 1000)]
        public int? NumberOfResults { get; set; }
    }

    public class GetEventLogEventsResponse
    {
        public string TenantId { get; set; } = String.Empty;
        public IEnumerable<EventResponse> Events { get; set; } = new List<EventResponse>();
        public int TotalEventCount { get; set; }
    }
}