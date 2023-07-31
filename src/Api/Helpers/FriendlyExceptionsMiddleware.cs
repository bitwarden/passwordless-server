using Datadog.Trace;
using FluentValidation;
using Microsoft.Data.Sqlite;
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
        var env = context.RequestServices.GetService<IWebHostEnvironment>();

        try
        {
            await _next(context);
        }
        catch (SqliteException e) when (env.IsDevelopment())
        {
            // this is done to allow migrations to be applied in development mode
            throw;
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
        catch (ValidationException validationException)
        {
            Tracer.Instance.ActiveScope?.Span.SetException(validationException);
            _logger.UncaughtException(validationException);

            await Results.ValidationProblem(
                validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray()),
                validationException.Message,
                validationException.Source,
                StatusCodes.Status400BadRequest,
                "One or more validation errors occured"
            ).ExecuteAsync(context);
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