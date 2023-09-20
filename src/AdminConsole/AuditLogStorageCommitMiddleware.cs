using Passwordless.AdminConsole.AuditLog.Loggers;

namespace Passwordless.AdminConsole;

public class AuditLogStorageCommitMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLogStorageCommitMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAuditLogger auditLogger)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            await auditLogger.FlushAsync();
        }
    }
}