using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Passwordless.AdminConsole.Services.PasswordlessClient;

public class ScopedApiSecretHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IOptionsMonitor<PasswordlessOptions> _optionsMonitor;

    public ScopedApiSecretHttpMessageHandler(IHttpContextAccessor accessor, IOptionsMonitor<PasswordlessOptions> optionsMonitor)
    {
        _accessor = accessor;
        _optionsMonitor = optionsMonitor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // We won't be able to access the request scope. HttpMessageHandlers
        var context = _accessor.HttpContext.RequestServices.GetRequiredService<ICurrentContext>();
        if (context.InAppContext)
        {
            request.Headers.Add("ApiSecret", context.ApiSecret);
        }
        else
        {
            var options = _optionsMonitor.CurrentValue;
            request.Headers.Add("ApiSecret", options.ApiSecret);
        }
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}