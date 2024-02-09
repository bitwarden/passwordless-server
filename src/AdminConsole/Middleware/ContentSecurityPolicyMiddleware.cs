using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Services.PasswordlessManagement;

namespace Passwordless.AdminConsole.Middleware;

public sealed class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PasswordlessManagementOptions _options;
    private readonly ILogger<ContentSecurityPolicyMiddleware> _logger;

    public ContentSecurityPolicyMiddleware(
        RequestDelegate next,
        IOptions<PasswordlessManagementOptions> options,
        ILogger<ContentSecurityPolicyMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public Task Invoke(HttpContext context)
    {
        // random bytes
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        if (context.Items.ContainsKey("csp-nonce"))
        {
            _logger.LogError("CSP nonce already exists in context.");
        }
        else
        {
            context.Items.Add("csp-nonce", nonce);
        }

        var csp =
            "default-src 'self';" +
            $"script-src 'self' 'unsafe-eval' 'nonce-{nonce}';" +
            $"connect-src 'self' {_options.ApiUrl};" +
            "style-src 'self' 'unsafe-inline';" +
            "img-src 'self' data:;" +
            "frame-ancestors 'none';" +
            "base-uri 'self';" +
            "object-src 'none'";

        context.Response.Headers.Append(
            "Content-Security-Policy",
            new[] { csp }
        );

        return _next(context);
    }
}