namespace Passwordless.AdminConsole.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public void Invoke(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", new[] { "nosniff" });
        context.Response.Headers.Append("Referrer-Policy", new[] { "no-referrer" });

        _next(context);
    }
}