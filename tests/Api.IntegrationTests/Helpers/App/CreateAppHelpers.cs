using System.Net.Http.Json;
using Bogus;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class CreateAppHelpers
{
    public static readonly Faker<AppCreateDTO> AppCreateGenerator = new Faker<AppCreateDTO>()
        .RuleFor(x => x.AdminEmail, x => x.Person.Email);

    public static async Task<HttpResponseMessage> CreateApplication(this HttpClient client, string applicationName)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        return await client.PostAsJsonAsync($"/admin/apps/{applicationName}/create", AppCreateGenerator.Generate());
    }
}