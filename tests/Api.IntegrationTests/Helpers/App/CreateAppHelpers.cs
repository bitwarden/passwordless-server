using System.Net.Http.Json;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class CreateAppHelpers
{
    public static string GetApplicationName() => $"test{Guid.NewGuid():N}";

    public static Task<HttpResponseMessage> CreateApplicationAsync(
        this HttpClient client,
        string applicationName,
        CreateAppDto? options = null)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        var actualOptions = options ?? new CreateAppDto();

        if (string.IsNullOrWhiteSpace(actualOptions.AdminEmail))
            actualOptions.AdminEmail = "admin@email.com";

        if (actualOptions.MagicLinkEmailMonthlyQuota == 0)
            actualOptions.MagicLinkEmailMonthlyQuota = 1000;

        return client.PostAsJsonAsync($"/admin/apps/{applicationName}/create", actualOptions);
    }

    public static Task<HttpResponseMessage> CreateApplicationAsync(this HttpClient client)
        => client.CreateApplicationAsync(GetApplicationName());
}