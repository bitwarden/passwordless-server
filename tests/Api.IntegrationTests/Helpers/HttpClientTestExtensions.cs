namespace Passwordless.Api.IntegrationTests.Helpers;

public static class HttpClientTestExtensions
{
    public const string ApiKey = "test:public:2e728aa5986f4ba8b073a5b28a939795";
    public const string ApiSecret = "test:secret:a679563b331846c79c20b114a4f56d02";
    
    public static bool HasPublicKey(this HttpClient client) =>
        client.DefaultRequestHeaders.Contains("ApiKey");

    public static HttpClient AddPublicKey(this HttpClient client) =>
        client.AddPublicKey(ApiKey);
    
    public static HttpClient AddPublicKey(this HttpClient client, string apiKey)
    {
        client.DefaultRequestHeaders.Add("ApiKey", apiKey);
        return client;
    }

    public static bool HasSecretKey(this HttpClient client) =>
        client.DefaultRequestHeaders.Contains("ApiSecret");
    
    public static HttpClient AddSecretKey(this HttpClient client) => client.AddSecretKey(ApiSecret);

    public static HttpClient AddSecretKey(this HttpClient client, string secretKey)
    {
        client.DefaultRequestHeaders.Add("ApiSecret", secretKey);
        return client;
    }

    public static HttpClient AddUserAgent(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
        return client;
    }

    public static bool HasManagementKey(this HttpClient client) =>
        client.DefaultRequestHeaders.Contains("ManagementKey");
    
    public static HttpClient AddManagementKey(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("ManagementKey", "shared_dev_key");
        return client;
    }

    public static HttpClient AddAcceptApplicationJson(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }
}