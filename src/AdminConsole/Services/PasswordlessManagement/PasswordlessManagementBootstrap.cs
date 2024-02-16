using Microsoft.Extensions.Options;

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
                bool isSelfHosted = config.GetValue("SelfHosted", false);
                if (!isSelfHosted)
                {
                    options.InternalApiUrl = options.ApiUrl;
                }
            })
            .ValidateOnStart();

        builder.Services.AddSingleton<PasswordlessHttpHandler>();
        builder.Services.AddHttpClient<IPasswordlessManagementClient, PasswordlessManagementClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PasswordlessManagementOptions>>().Value;

            client.BaseAddress = new Uri(options.InternalApiUrl);
            client.DefaultRequestHeaders.Add("ManagementKey", options.ManagementKey);
        }).ConfigurePrimaryHttpMessageHandler<PasswordlessHttpHandler>();
    }
}