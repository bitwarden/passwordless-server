using Passwordless.AspNetCore;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePasswordless(this IApplicationBuilder app, PasswordlessEndpointOptions? options = null)
    {
        options ??= new PasswordlessEndpointOptions();
        app.UseMiddleware<PasswordlessMiddleware>(options);
        return app;
    }
}