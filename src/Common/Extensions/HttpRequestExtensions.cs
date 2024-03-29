using System.Text;

namespace Passwordless.Common.Extensions;

public static class HttpRequestExtensions
{
    public static string GetBaseUrl(this HttpRequest request)
    {
        var uriBuilder = new StringBuilder();
        uriBuilder.Append(request.Scheme);
        uriBuilder.Append("://");
        uriBuilder.Append(request.Host.Value);
        return uriBuilder.ToString();
    }
}