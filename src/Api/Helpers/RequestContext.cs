using System.Text;

namespace Passwordless.Api.Helpers;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetBaseUrl()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new InvalidOperationException();
        }
        var request = _httpContextAccessor.HttpContext.Request;
        var uriBuilder = new StringBuilder();
        uriBuilder.Append(request.Scheme);
        uriBuilder.Append("://");
        uriBuilder.Append(request.Host.Value);
        return uriBuilder.ToString();
    }
}