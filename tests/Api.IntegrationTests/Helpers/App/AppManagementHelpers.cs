using System.Net.Http.Json;
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
}