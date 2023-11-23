using Microsoft.Extensions.Options;
using Passwordless.Common.Configuration;

namespace Passwordless.AdminConsole.Services.PasswordlessManagement;

public static class PasswordlessManagementBootstrap
{
    public static void AddManagementApi(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<PasswordlessManagementOptions>()
            .BindConfiguration("PasswordlessManagement")
            .Configure<IConfiguration>((options, config) =>
            {
                bool isSelfHosted = config.IsSelfHosted();
                if (!isSelfHosted)
                {
                    options.InternalApiUrl = options.ApiUrl;
                }
            })
            .ValidateOnStart();

        builder.Services.AddHttpClient<IPasswordlessManagementClient, PasswordlessManagementClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PasswordlessManagementOptions>>().Value;

            client.BaseAddress = new Uri(options.InternalApiUrl);
            client.DefaultRequestHeaders.Add("ManagementKey", options.ManagementKey);
        });
    }
}