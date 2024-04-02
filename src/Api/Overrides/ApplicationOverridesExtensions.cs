namespace Passwordless.Api.Overrides;

/// <summary>
/// Extensions for <see cref="ApplicationOverrides" />.
/// </summary>
public static class ApplicationOverridesExtensions
{
    /// <summary>
    /// Gets overrides for the specified application from the configuration.
    /// </summary>
    public static ApplicationOverrides? TryGetApplicationOverrides(
        this IConfiguration configuration,
        string applicationId) =>
        configuration
            .GetSection(applicationId)
            .Get<ApplicationOverrides>();
}