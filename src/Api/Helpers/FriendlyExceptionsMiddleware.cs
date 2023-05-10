using Datadog.Trace;
using Passwordless.Service.Helpers;

namespace Passwordless.Api.Helpers;

public class FriendlyExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public FriendlyExceptionsMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        return SafeNext(context);
    }

    private async Task SafeNext(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException apiException)
        {
            Tracer.Instance.ActiveScope?.Span.SetException(apiException);
            _logger.UncaughtApiException(apiException);

            var extras = apiException.Extras ?? new Dictionary<string, object>();

            extras.TryAdd("errorCode", apiException.ErrorCode);

            var problem = Results.Problem(
                statusCode: apiException.StatusCode,
                title: apiException.Message,
                extensions: extras,
                type: $"https://docs.passwordless.dev/guide/errors.html#{apiException.ErrorCode}");
            await problem.ExecuteAsync(context);
        }
        catch (Exception exception)
        {
            Tracer.Instance.ActiveScope?.Span.SetException(exception);
            _logger.UncaughtException(exception);
            var problem = Results.Problem("The api has encountered an error", statusCode: 500);
            await problem.ExecuteAsync(context);
        }
    }
}