using System.Net.Http.Json;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class CreateAppHelpers
{
    public static string GetApplicationName() => $"test{Guid.NewGuid():N}";

    public static Task<HttpResponseMessage> CreateApplicationAsync(
        this HttpClient client,
        string applicationName,
        string? adminEmail = null)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        return client.PostAsJsonAsync($"/admin/apps/{applicationName}/create", new CreateAppDto
        {
            AdminEmail = adminEmail ?? "test-admin@passwordless.dev"
        });
    }

    public static Task<HttpResponseMessage> CreateApplicationAsync(this HttpClient client)
        => client.CreateApplicationAsync(GetApplicationName());
}