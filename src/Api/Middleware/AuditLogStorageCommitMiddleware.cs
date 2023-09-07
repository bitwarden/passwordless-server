using Passwordless.Service.AuditLog.Loggers;

namespace Passwordless.Api.Middleware;

public class AuditLogStorageCommitMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLogStorageCommitMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAuditLogger auditLogger)
    {
        await _next(context);
        await auditLogger.FlushAsync();
    }
}