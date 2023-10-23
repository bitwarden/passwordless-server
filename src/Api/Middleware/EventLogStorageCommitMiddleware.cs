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

    public async Task InvokeAsync(HttpContext context, IServiceProvider provider)
    {
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