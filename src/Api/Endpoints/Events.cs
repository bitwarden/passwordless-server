using Passwordless.Api.Authorization;
using Passwordless.Api.Models;

namespace Passwordless.Server.Endpoints;

public static class AuditLog
{
    public static void MapAuditLogEndpoints(this WebApplication app)
    {
        app.MapGet("events", async () =>
        {
            
        }).RequireManagementKey()
            .RequireCors("default");
        
        app.MapPost("events", async (AuditEventRequest request) =>
        {
            
        }).RequireManagementKey()
            .RequireCors("default");

        app.MapPost("events/app", async (AppAuditEventRequest request) =>
        {
        }).RequireSecretKey()
            .RequireCors("default");
    }
}