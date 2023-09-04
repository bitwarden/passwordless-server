using Microsoft.Extensions.Options;
using Passwordless.AdminConsole.Services;

namespace AdminConsole;

public static class ManagementApiExtensions
{
    public static IServiceCollection AddManagementApi(this IServiceCollection services)
    {
        services.AddOptions<PasswordlessManagementOptions>()
            .BindConfiguration("PasswordlessManagement");

        services.AddHttpClient<IPasswordlessManagementClient, PasswordlessManagementClient>((sp, client) =>
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
    private string _publicUrl;
    public string PublicUrl
    {
        get => string.IsNullOrEmpty(_publicUrl) ? ApiUrl : _publicUrl;
        set
        {
            _publicUrl = value;
        }
    }

    public int PublicApiPort { get; set; }

    public string ApiUrl { get; set; }
    public string ManagementKey { get; set; }

    public string PublicApiUrl
    {
        get
        {
            if (string.IsNullOrEmpty(_publicUrl))
            {
                return ApiUrl;
            }

            return $"{PublicUrl}:{PublicApiPort}";
        }
    }
}