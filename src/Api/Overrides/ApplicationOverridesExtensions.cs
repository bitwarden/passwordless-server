namespace Passwordless.Api.Overrides;

/// <summary>
/// Extensions for <see cref="ApplicationOverrides" />.
/// </summary>
public static class ApplicationOverridesExtensions
{
    /// <summary>
    /// Gets overrides for the specified application from the configuration.
    /// </summary>
    public static ApplicationOverrides GetApplicationOverrides(
        this IConfiguration configuration,
        string applicationId) =>
        configuration
            .GetSection(applicationId)
            .Get<ApplicationOverrides>() ?? new ApplicationOverrides();
}