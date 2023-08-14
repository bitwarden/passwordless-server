namespace Passwordless.AdminConsole;

public class ScopedApiSecretDelegatingHandler : DelegatingHandler
{
    private readonly IScopedPasswordlessContext _context;
    
    public ScopedApiSecretDelegatingHandler(IScopedPasswordlessContext context) => _context = context;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("ApiSecret", _context.ApiSecret);
        return base.SendAsync(request, cancellationToken);
    }
}