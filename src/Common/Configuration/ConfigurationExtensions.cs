namespace Passwordless.Common.Configuration;

public static class ConfigurationExtensions
{
    private static readonly string SelfHosted = "SelfHosted";

    public static bool IsSelfHosted(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>(SelfHosted, false);
    }
}