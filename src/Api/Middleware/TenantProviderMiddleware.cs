using ApiHelpers;
using Passwordless.Service;

namespace Passwordless.Api.Middleware;

public class TenantProviderMiddleware
{
    private readonly RequestDelegate _next;

    public TenantProviderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // IMessageWriter is injected into InvokeAsync
    public async Task InvokeAsync(HttpContext httpContext, ITenantProvider tenantProvider)
    {
        var name = httpContext.Request.GetTenantName();

        tenantProvider.Tenant = name;
        await _next(httpContext);
    }
}