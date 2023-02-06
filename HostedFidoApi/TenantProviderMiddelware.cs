using Service;
using ApiHelpers;

public class TenantProviderMiddelware
{
    private readonly RequestDelegate _next;

    public TenantProviderMiddelware(RequestDelegate next)
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