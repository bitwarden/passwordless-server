using System.Net.Http.Json;
using Passwordless.Api.Endpoints;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.MagicLinks.Models;

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
    
    public static async Task<HttpClient> EnableMagicLinks(this HttpClient client, string applicationName, string performedBy)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        _ = await client.PostAsJsonAsync($"admin/apps/{applicationName}/magic-links/enable",
            new EnableMagicLinksRequest(performedBy));

        return client;
    }

    public static async Task<HttpClient> DisableMagicLinks(this HttpClient client, string applicationName, string performedBy)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        _ = (await client.PostAsJsonAsync($"admin/apps/{applicationName}/magic-links/disable",
            new DisableMagicLinksRequest(performedBy))).EnsureSuccessStatusCode();
        
        return client;
    }
}