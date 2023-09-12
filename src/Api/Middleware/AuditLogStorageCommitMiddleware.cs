using Passwordless.Service.AuditLog.Loggers;

namespace Passwordless.Api.Middleware;

public class AuditLogStorageCommitMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLogStorageCommitMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AuditLoggerProvider auditLoggerProvider)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            var logger = await auditLoggerProvider.Create();
            await logger.FlushAsync();
        }
    }
}