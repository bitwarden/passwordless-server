namespace Passwordless.Api.Integration.Tests.Helpers;

public static class HttpClientTestExtensions
{
    public static HttpClient AddPublicKey(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("ApiKey", "test:public:2e728aa5986f4ba8b073a5b28a939795");
        return client;
    }

    public static HttpClient AddSecretKey(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("ApiSecret", "test:secret:a679563b331846c79c20b114a4f56d02");
        return client;
    }

    public static HttpClient AddUserAgent(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
        return client;
    }

    public static HttpClient AddManagementKey(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("ManagementKey", "shared_dev_key");
        return client;
    }

}