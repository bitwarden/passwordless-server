using Datadog.Trace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Passwordless.Service.Helpers;

namespace Passwordless.Api.Helpers;

public class FriendlyExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public FriendlyExceptionsMiddleware(RequestDelegate next,
        ILogger<LoggingMiddleware> logger,
        IProblemDetailsService problemDetailsService)
    {
        _next = next;
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public Task InvokeAsync(HttpContext context)
    {
        return SafeNextAsync(context);
    }

    private async Task SafeNextAsync(HttpContext context)
    {
        var env = context.RequestServices.GetService<IWebHostEnvironment>();

        try
        {
            await _next(context);
        }
        catch (SqliteException e) when (env.IsDevelopment())
        {
            // This is done to allow migrations to be applied in development mode
            throw;
        }
        catch (ApiException apiException)
        {
            Tracer.Instance.ActiveScope?.Span.SetException(apiException);
            _logger.UncaughtApiException(apiException);

            var problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = apiException.StatusCode,
                    Title = apiException.Message,
                    Type = $"https://docs.passwordless.dev/guide/errors.html#{apiException.ErrorCode}"
                }
            };

            var extras = apiException.Extras ?? new Dictionary<string, object>();

            extras.TryAdd("errorCode", apiException.ErrorCode);

            foreach (var pair in extras)
            {
                problemDetailsContext.ProblemDetails.Extensions.TryAdd(pair.Key, pair.Value);
            }

            context.Response.StatusCode = apiException.StatusCode;
            await _problemDetailsService.WriteAsync(problemDetailsContext);
        }
        // 4xx errors produced by the framework
        catch (BadHttpRequestException badRequestException)
        {
            Tracer.Instance.ActiveScope?.Span.SetException(badRequestException);
            _logger.UncaughtException(badRequestException);

            context.Response.StatusCode = badRequestException.StatusCode;
            await _problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = badRequestException.StatusCode,
                    Title = badRequestException.Message
                }
            });
        }
        catch (Exception exception)
        {
            Tracer.Instance.ActiveScope?.Span.SetException(exception);
            _logger.UncaughtException(exception);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await _problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Detail = "The api has encountered an error",
                    Status = StatusCodes.Status500InternalServerError
                }
            });
        }
    }
}