using Passwordless.Api.Helpers;

namespace Passwordless.Api.Middleware;

public class AcceptHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public AcceptHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        httpContext.AddProblemDetailsMissingAcceptHeader();

        await _next(httpContext);
    }
}