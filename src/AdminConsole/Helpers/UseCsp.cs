using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace AdminConsole.Helpers;

public static class UseCspExtensions
{
    public static IApplicationBuilder UseCSP(this IApplicationBuilder app)
    {
        app.Use((context, next) =>
        {
            // random bytes
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            if (context.Items.ContainsKey("csp-nonce"))
            {
                var logger = app.ApplicationServices.GetService<ILogger>();
                logger?.LogError("CSP nonce already exists in context.");
            }
            else
            {
                context.Items.Add("csp-nonce", nonce);
            }

            var passConfig = context.RequestServices.GetService<IOptions<PasswordlessOptions>>();
            var csp = $"""
                    default-src 'self';
                    script-src cdn.passwordless.dev 'self' 'unsafe-eval' 'nonce-{nonce}';
                    connect-src 'self' {passConfig.Value.ApiUrl};
                    style-src 'self' 'unsafe-inline';
                    """.Replace("\r\n", " ");

            context.Response.Headers.Add("Content-Security-Policy",
                new[] { csp });
            return next();
        });

        return app;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use((context, next) =>
        {
            context.Response.Headers.Add("X-Content-Type-Options", new[] { "nosniff" });
            context.Response.Headers.Add("Referrer-Policy", new[] { "no-referrer" });
            return next();
        });

        return app;
    }
}