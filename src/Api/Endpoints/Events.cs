using Passwordless.Api.Authorization;
using Passwordless.Api.Models;
using Passwordless.Common.AuditLog;
using Passwordless.Service.AuditLog;
using Passwordless.Service.AuditLog.Mappings;

namespace Passwordless.Server.Endpoints;

public static class AuditLog
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        app.MapGet("events/{appId}", async (string appId, IAuditLoggerStorageFactory auditLoggerStorageFactory, CancellationToken cancellationToken) =>
            new
            {
                TenantId = appId,
                Events = (await auditLoggerStorageFactory
                    .Create()
                    .GetAuditLogAsync(appId, cancellationToken))
                    .Select(x => x.ToEvent())
            }).RequireSecretKey()
            .RequireCors("default");

        app.MapPost("events/app", async (AppAuditEventRequest request) =>
            {
            }).RequireSecretKey()
            .RequireCors("default");
    }
}