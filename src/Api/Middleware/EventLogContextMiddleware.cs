using Passwordless.Api.Extensions;
using Passwordless.Common.Models;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Features;

namespace Passwordless.Api.Middleware;

public class EventLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public EventLogContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IEventLogContext eventLogContext, IFeatureContextProvider featureContextProvider)
    {
        var tenantId = context.Request.GetTenantName();
        var requestKey = context.Request.GetPublicApiKey() ?? context.Request.GetApiSecret();

        var featuresContext = await featureContextProvider.UseContext();

        switch (requestKey)
        {
            case not null when requestKey.Contains(ApplicationPublicKey.KeyIdentifier):
                eventLogContext.SetContext(tenantId, featuresContext, new ApplicationPublicKey(requestKey));
                break;
            case not null when requestKey.Contains(ApplicationSecretKey.KeyIdentifier):
                eventLogContext.SetContext(tenantId, featuresContext, new ApplicationSecretKey(requestKey));
                break;
            default:
                eventLogContext.SetContext(tenantId, featuresContext);
                break;
        }

        await _next(context);
    }
}