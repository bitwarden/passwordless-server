namespace Passwordless.Common.Overrides;

public class ApplicationOverridesOptions : Dictionary<string, ApplicationOverrides>
{
    /// <summary>
    /// Gets the application overrides for the specified tenant. If the application is not found, an instance with
    /// default values is returned.
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public ApplicationOverrides GetApplication(string tenant)
    {
        return this.TryGetValue(tenant, out var overrides) ? overrides : new ApplicationOverrides();
    }
}