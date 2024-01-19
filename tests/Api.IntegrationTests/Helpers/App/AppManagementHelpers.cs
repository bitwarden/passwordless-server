using System.Net.Http.Json;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class AppManagementHelpers
{
    public static async Task<HttpClient> EnableEventLogging(this HttpClient client, string applicationName)
    {
        client.AddManagementKey();
        _ = await client.PostAsJsonAsync($"admin/apps/{applicationName}/features", new ManageFeaturesRequest
        {
            EventLoggingRetentionPeriod = 7,
            EventLoggingIsEnabled = true,
            MaxUsers = null
        });

        return client;
    }

    public static async Task<HttpClient> EnableAttestation(this HttpClient client, string applicationName)
    {
        client.AddManagementKey();
        var response = await client.PostAsJsonAsync($"admin/apps/{applicationName}/features", new ManageFeaturesRequest
        {
            AllowAttestation = true
        });

        response.EnsureSuccessStatusCode();

        return client;
    }

    public static async Task<HttpClient> EnableManuallyGenerateAccessTokenEndpoint(this HttpClient client, string performedBy)
    {
        if (!client.HasSecretKey()) throw new Exception("Secret key must be set on HttpClient");
        _ = (await client.PostAsJsonAsync("apps/features",
            new SetFeaturesRequest
            {
                PerformedBy = performedBy,
                EnableManuallyGeneratedAuthenticationTokens = true
            })).EnsureSuccessStatusCode();

        return client;
    }

    public static async Task<HttpClient> DisableManuallyGenerateAccessTokenEndpoint(this HttpClient client, string performedBy)
    {
        if (!client.HasSecretKey()) throw new Exception("Secret key must be set on HttpClient");
        _ = (await client.PostAsJsonAsync("apps/features",
            new SetFeaturesRequest
            {
                PerformedBy = performedBy,
                EnableManuallyGeneratedAuthenticationTokens = false
            })).EnsureSuccessStatusCode();

        return client;
    }

    public static async Task<HttpClient> EnableMagicLinks(this HttpClient client, string performedBy)
    {
        if (!client.HasSecretKey()) throw new Exception("Secret key must be set on HttpClient");
        _ = (await client.PostAsJsonAsync("apps/features",
            new SetFeaturesRequest
            {
                PerformedBy = performedBy,
                EnableMagicLinks = true
            })).EnsureSuccessStatusCode();

        return client;
    }

    public static async Task<HttpClient> DisableMagicLinks(this HttpClient client, string performedBy)
    {
        if (!client.HasSecretKey()) throw new Exception("Secret key must be set on HttpClient");
        _ = (await client.PostAsJsonAsync("apps/features",
            new SetFeaturesRequest
            {
                PerformedBy = performedBy,
                EnableMagicLinks = false
            })).EnsureSuccessStatusCode();

        return client;
    }
}