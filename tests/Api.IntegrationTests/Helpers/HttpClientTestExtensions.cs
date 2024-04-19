namespace Passwordless.Api.IntegrationTests.Helpers;

public static class HttpClientTestExtensions
{
    private const string ManagementKeyHeaderKey = "ManagementKey";
    private const string ApiSecretHeaderKey = "ApiSecret";
    private const string ApiKeyHeaderKey = "ApiKey";

    public static bool HasPublicKey(this HttpClient client) =>
        client.DefaultRequestHeaders.Contains(ApiKeyHeaderKey);

    public static HttpClient AddPublicKey(this HttpClient client, string apiKey)
    {
        if (client.HasPublicKey()) client.DefaultRequestHeaders.Remove(ApiKeyHeaderKey);
        client.DefaultRequestHeaders.Add(ApiKeyHeaderKey, apiKey);
        return client;
    }

    public static bool HasSecretKey(this HttpClient client) =>
        client.DefaultRequestHeaders.Contains(ApiSecretHeaderKey);

    public static HttpClient AddSecretKey(this HttpClient client, string secretKey)
    {
        if (client.HasSecretKey()) client.DefaultRequestHeaders.Remove(ApiSecretHeaderKey);
        client.DefaultRequestHeaders.Add(ApiSecretHeaderKey, secretKey);
        return client;
    }

    public static HttpClient AddUserAgent(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
        return client;
    }

    public static bool HasManagementKey(this HttpClient client) => client.DefaultRequestHeaders.Contains(ManagementKeyHeaderKey);

    public static HttpClient AddManagementKey(this HttpClient client)
    {
        if (client.HasManagementKey()) client.DefaultRequestHeaders.Remove(ManagementKeyHeaderKey);
        client.DefaultRequestHeaders.Add(ManagementKeyHeaderKey, "shared_dev_key");
        return client;
    }

    public static HttpClient AddAcceptApplicationJson(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }
}