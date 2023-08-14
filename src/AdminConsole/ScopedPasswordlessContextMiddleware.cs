namespace Passwordless.AdminConsole;

public class ScopedPasswordlessContextMiddleware
{
    private readonly RequestDelegate _next;

    public ScopedPasswordlessContextMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context, ICurrentContext currentContext, IScopedPasswordlessContext scopedPasswordlessContext)
    {
        if (currentContext == null || string.IsNullOrWhiteSpace(currentContext.ApiSecret)) return _next(context);

        scopedPasswordlessContext.ApiSecret = currentContext.ApiSecret;

        return _next(context);
    }
}