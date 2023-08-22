using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service.AuditLog.Loggers;

namespace Passwordless.Server.Endpoints;

public static class AuditLog
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        app.MapGet("events/{appId}", async () =>
            {
            }).RequireManagementKey()
            .RequireCors("default");

        app.MapGet("events/{organizationId}", async () =>
            {
            }).RequireManagementKey()
            .RequireCors("default");

        app.MapPost("events", async (AuditEventRequest request, IAuditLoggerFactory factory, HttpRequest httpRequest) =>
            {
                var auditLogger = await factory.Create();
                await auditLogger.LogEvent(request.ToEvent());
            }).RequireManagementKey()
            .RequireCors("default");

        app.MapPost("events/app", async (AppAuditEventRequest request) =>
            {
            }).RequireSecretKey()
            .RequireCors("default");
    }
}