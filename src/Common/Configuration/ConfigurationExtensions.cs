namespace Passwordless.Common.Configuration;

public static class ConfigurationExtensions
{
    public static bool IsSelfHosted(this IConfiguration configuration)
    {
        return configuration.GetValue("SelfHosted", false);
    }
}