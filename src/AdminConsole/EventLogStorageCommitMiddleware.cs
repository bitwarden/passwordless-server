using Passwordless.AdminConsole.EventLog.Loggers;

namespace Passwordless.AdminConsole;

public class EventLogStorageCommitMiddleware
{
    private readonly RequestDelegate _next;

    public EventLogStorageCommitMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IEventLogger eventLogger)
    {
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