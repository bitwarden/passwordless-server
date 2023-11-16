namespace Passwordless.Api.Helpers;

public static class SecurityHeaders
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use((context, next) =>
        {
            const string csp = "default-src 'self';";
            context.Response.Headers.Append("Content-Security-Policy", new[] { csp });
            context.Response.Headers.Append("X-Content-Type-Options", new[] { "nosniff" });
            context.Response.Headers.Append("Referrer-Policy", new[] { "no-referrer" });
            return next();
        });

        return app;
    }
}