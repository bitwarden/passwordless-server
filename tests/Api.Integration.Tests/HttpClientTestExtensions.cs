namespace Passwordless.Api.Integration.Tests;

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
}