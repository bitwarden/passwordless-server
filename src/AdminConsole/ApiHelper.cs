using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Models.DTOs;
using Passwordless.AdminConsole.Services;
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