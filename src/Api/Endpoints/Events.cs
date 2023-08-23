using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Service.AuditLog;
using Passwordless.Service.AuditLog.Loggers;

namespace Passwordless.Server.Endpoints;

public static class AuditLog
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        app.MapGet("events/{appId}", async (string appId) =>
            {
            }).RequireManagementKey()
            .RequireCors("default");

        app.MapGet("events/{organizationId:int}", async (int organizationId, IAuditLoggerStorageFactory auditLoggerStorageFactory, CancellationToken cancellationToken) =>
                new
                {
                    OrganizationId = organizationId,
                    Events = (await auditLoggerStorageFactory
                            .Create()
                            .GetAuditLogAsync(organizationId, cancellationToken))
                        .Select(x => x.ToEvent())
                })
            .RequireManagementKey()
            .RequireCors("default");

        app.MapPost("events", async (AuditEventRequest request, IAuditLoggerFactory factory) =>
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