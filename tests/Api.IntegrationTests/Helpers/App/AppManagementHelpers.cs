using System.Net.Http.Json;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class AppManagementHelpers
{
    public static async Task<HttpClient> EnableEventLogging(this HttpClient client, string applicationName)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        _ = await client.PostAsJsonAsync($"admin/apps/{applicationName}/features", new ManageFeaturesDto
        {
            EventLoggingRetentionPeriod = 7,
            EventLoggingIsEnabled = true,
            MaxUsers = null
        });

        return client;
    }
}