using System.Net.Http.Json;
using Bogus;
using Passwordless.Service.Models;

namespace Passwordless.Api.IntegrationTests.Helpers.App;

public static class CreateAppHelpers
{
    public static readonly Faker<AppCreateDTO> AppCreateGenerator = new Faker<AppCreateDTO>()
        .RuleFor(x => x.AdminEmail, x => x.Person.Email);

    public static string GetApplicationName() => $"test{Guid.NewGuid():N}";

    public static Task<HttpResponseMessage> CreateApplicationAsync(this HttpClient client, string applicationName)
    {
        if (!client.DefaultRequestHeaders.Contains("ManagementKey"))
        {
            client.AddManagementKey();
        }

        return client.PostAsJsonAsync($"/admin/apps/{applicationName}/create", AppCreateGenerator.Generate());
    }

    public static Task<HttpResponseMessage> CreateApplicationAsync(this HttpClient client)
        => client.CreateApplicationAsync(GetApplicationName());
}