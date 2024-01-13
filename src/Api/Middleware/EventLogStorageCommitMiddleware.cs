using Passwordless.Service.EventLog.Loggers;

namespace Passwordless.Api.Middleware;

public class EventLogStorageCommitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EventLogStorageCommitMiddleware> _logger;

    public EventLogStorageCommitMiddleware(RequestDelegate next, ILogger<EventLogStorageCommitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// This middleware will commit any events that were created in the request to the database via the `IEventLogger`.
    /// </summary>
    /// <param name="context">The `HttpContext` from the request.</param>
    /// <param name="provider">Used to get the `IEventLogger` instance based on the current state of the request.</param>
    public async Task InvokeAsync(HttpContext context, IServiceProvider provider)
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
            try
            {
                await provider.GetRequiredService<IEventLogger>().FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write event log to db.");
            }
        }
    }
}