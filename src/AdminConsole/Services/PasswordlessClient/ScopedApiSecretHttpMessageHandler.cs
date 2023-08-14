namespace Passwordless.AdminConsole.Services.PasswordlessClient;

public class ScopedApiSecretHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public ScopedApiSecretHttpMessageHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // We won't be able to access the request scope. HttpMessageHandlers
        var context = _accessor.HttpContext.RequestServices.GetRequiredService<ICurrentContext>();
        request.Headers.Add("ApiSecret", context.ApiSecret);
        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}