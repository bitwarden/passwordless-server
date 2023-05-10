using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace AdminConsole;

public static class ManagementApiExtensions
{
    public static IServiceCollection AddManagementApi(this IServiceCollection services)
    {
        services.AddOptions<PasswordlessManagementOptions>()
            .BindConfiguration("PasswordlessManagement");

        services.AddHttpClient<PasswordlessManagementClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PasswordlessManagementOptions>>().Value;

            client.BaseAddress = new Uri(options.ApiUrl);
            client.DefaultRequestHeaders.Add("ManagementKey", options.ManagementKey);
        });

        return services;
    }
}

public class PasswordlessManagementOptions
{
    public string ApiUrl { get; set; }
    public string ManagementKey { get; set; }
}

public class PasswordlessManagementClient
{
    private readonly HttpClient _client;

    public PasswordlessManagementClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<NewAppResponse> CreateApplication(NewAppOptions registerOptions)
    {
        var res = await _client.PostAsJsonAsync("apps/create", registerOptions);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<NewAppResponse>();
    }
}