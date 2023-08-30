using Passwordless.Net;

namespace Passwordless.AdminConsole.Services;

public interface IScopedPasswordlessClient : IPasswordlessClient
{

}

public class ScopedPasswordlessClient : PasswordlessClient, IScopedPasswordlessClient
{
    public ScopedPasswordlessClient(HttpClient httpClient, ICurrentContext context) : base(httpClient)
    {
        httpClient.DefaultRequestHeaders.Remove("ApiSecret");
        httpClient.DefaultRequestHeaders.Add("ApiSecret", context.ApiSecret);
    }
}