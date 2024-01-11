using System.Net.Http.Json;
using Passwordless.Api.Endpoints;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class AppManagementHelpers
{
    public static async Task<HttpClient> EnableEventLogging(this HttpClient client, string applicationName)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        _ = await client.PostAsJsonAsync($"admin/apps/{applicationName}/features", new ManageFeaturesRequest
        {
            EventLoggingRetentionPeriod = 7,
            EventLoggingIsEnabled = true,
            MaxUsers = null
        });

        return client;
    }

    public static async Task<HttpClient> EnableManuallyGenerateAccessTokenEndpoint(this HttpClient client, string applicationName, string performedBy)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        _ = await client.PostAsJsonAsync($"admin/apps/{applicationName}/sign-in-generate-token-endpoint/enable",
            new AppsEndpoints.EnableGenerateSignInTokenEndpointRequest(performedBy));

        return client;
    }

    public static async Task<HttpClient> DisableManuallyGenerateAccessTokenEndpoint(this HttpClient client, string applicationName, string performedBy)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        _ = await client.PostAsJsonAsync($"admin/apps/{applicationName}/sign-in-generate-token-endpoint/disable",
            new AppsEndpoints.DisableGenerateSignInTokenEndpointRequest(performedBy));

        return client;
    }
}