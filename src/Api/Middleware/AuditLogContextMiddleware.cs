using ApiHelpers;
using Passwordless.Common.Models;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Features;

namespace Passwordless.Api.Middleware;

public class AuditLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLogContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAuditLogContext auditLogContext, IFeatureContextProvider featureContextProvider)
    {
        var tenantId = context.Request.GetTenantName();
        var requestKey = context.Request.GetPublicApiKey() ?? context.Request.GetApiSecret();

        var featuresContext = await featureContextProvider.UseContext();

        switch (requestKey)
        {
            case not null when requestKey.Contains("public"):
                auditLogContext.SetContext(tenantId, featuresContext, new ApplicationPublicKey(requestKey));
                break;
            case not null when requestKey.Contains("secret"):
                auditLogContext.SetContext(tenantId, featuresContext, new ApplicationSecretKey(requestKey));
                break;
            default:
                auditLogContext.SetContext(tenantId, featuresContext);
                break;
        }

        await _next(context);
    }
}