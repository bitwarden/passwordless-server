using System.Net.Http.Json;
using Passwordless.Common.Models.Apps;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class CreateAppHelpers
{
    public static string GetApplicationName() => $"test{Guid.NewGuid():N}";

    public static async Task<HttpResponseMessage> CreateApplicationRawAsync(
        this HttpClient client,
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

        return await client.PostAsJsonAsync($"/admin/apps/create", actualOptions);
    }

    public static async Task<CreateAppResultDto> CreateApplicationAsync(
        this HttpClient client,
        CreateAppDto? options = null)
    {
        using var response = await client.CreateApplicationRawAsync(options);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateAppResultDto>() ??
               throw new InvalidOperationException("Empty response");
    }
}