using Microsoft.Extensions.Primitives;

namespace Passwordless.Common.Middleware.SelfHosting;

/**
 * Because we're running behind a reverse proxy in HTTP context.
 * We need to set the overwrite protocol again.
 */
public class HttpOverridesMiddleware
{
    private readonly RequestDelegate _next;

    public HttpOverridesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // IMessageWriter is injected into InvokeAsync
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-Proto", out StringValues header))
        {
            string forwardedProto = header.FirstOrDefault();

            if (forwardedProto == "https")
            {
                context.Request.Scheme = "https";
            }
        }

        await _next(context);
    }
}