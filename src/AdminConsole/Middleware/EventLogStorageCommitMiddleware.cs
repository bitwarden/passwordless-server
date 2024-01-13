using Passwordless.AdminConsole.EventLog.Loggers;

namespace Passwordless.AdminConsole.Middleware;

public partial class EventLogStorageCommitMiddleware
{
    private readonly RequestDelegate _next;

    public EventLogStorageCommitMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IEventLogger eventLogger)
    {
        if (context.GetEndpoint() == null)
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            await eventLogger.FlushAsync();
        }
    }
}