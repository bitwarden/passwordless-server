using ApiHelpers;
using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Common.AuditLog.Models;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.AuditLog.Mappings;
using Passwordless.Service.Helpers;

namespace Passwordless.Server.Endpoints;

public static class AuditLog
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        app.MapGet("events", async (
                    int pageNumber,
                    int numberOfResults,
                    HttpRequest request,
                    IAuditLogStorage storage,
                    CancellationToken cancellationToken) =>
                {
                    var tenantId = request.GetTenantName();

                    return new
                    {
                        TenantId = tenantId,
                        Events = (await storage
                                .GetAuditLogAsync(tenantId, pageNumber, numberOfResults, cancellationToken))
                            .Select(x => x.ToEvent()),
                        TotalEventCount = await storage.GetAuditLogCountAsync(tenantId, cancellationToken)
                    };
                })
            .RequireSecretKey()
            .RequireCors("default");

        app.MapPost("events", async (AppAuditEventRequest eventRequest, HttpRequest httpRequest, AuditLoggerProvider auditLogger) =>
                (await auditLogger.Create()).LogEvent(eventRequest.ToDto(httpRequest)))
            .RequireSecretKey()
            .RequireCors("default");
    }

    private static AuditEventDto ToDto(this AppAuditEventRequest eventRequest, HttpRequest httpRequest) => new()
    {
        Message = eventRequest.Message,
        PerformedBy = eventRequest.PerformedBy,
        EventType = eventRequest.EventType,
        Severity = eventRequest.Severity,
        Subject = eventRequest.Subject,
        TenantId = httpRequest.GetTenantName(),
        ApiKeyId = httpRequest.GetApiSecret().GetLast(4)
    };
}