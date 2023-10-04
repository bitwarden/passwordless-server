using Passwordless.Service.EventLog.Loggers;

namespace Passwordless.Api.Middleware;

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