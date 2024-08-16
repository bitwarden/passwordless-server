using System.Reflection;
using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Components.Pages.App;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App;

/// <summary>
/// This is verifying that all application pages inherit from <c href="BaseApplicationPage" /> and that <c href="HasAppHandler" /> authorization policy is applied.
/// </summary>
public class BaseApplicationTests
{
    /// <summary>
    /// Verify that all application pages inherit from <c href="BaseApplicationPage" /> to ensure consistency.
    /// </summary>
    [Fact]
    public void AllApplicationPages_Inherit_BaseApplicationPage()
    {
        var assembly = Assembly.GetAssembly(typeof(AdminConsole.Components.App))!;

        var componentTypes = assembly.GetTypes()
            .Where(type =>
                typeof(ComponentBase).IsAssignableFrom(type) &&
                type.Namespace!.StartsWith("Passwordless.AdminConsole.Components.Pages.App"));

        foreach (var componentType in componentTypes)
        {
            // Check if the component has a @page directive
            var pageAttributes = componentType.GetCustomAttributes<RouteAttribute>();

            if (pageAttributes.Any())
            {
                // Assert that the component inherits from BaseApplicationPage
                Assert.True(typeof(BaseApplicationPage).IsAssignableFrom(componentType),
                    $"{componentType.Name} should inherit from BaseApplicationPage");
            }
        }
    }

    /// <summary>
    /// Verify that all application pages whose route starts with '/app/{app}' inherit from <c href="BaseApplicationPage" /> to ensure consistency.
    /// </summary>
    [Fact]
    public void AllApplicationPages_Inherit_BaseApplicationPage_WhenPathMatches()
    {
        var assembly = Assembly.GetAssembly(typeof(AdminConsole.Components.App))!;

        var componentTypes = assembly.GetTypes()
            .Where(type =>
                typeof(Microsoft.AspNetCore.Components.ComponentBase).IsAssignableFrom(type));

        foreach (var componentType in componentTypes)
        {
            // Check if the component has a @page directive
            var pageAttribute = componentType.GetCustomAttribute<Microsoft.AspNetCore.Components.RouteAttribute>();
            if (pageAttribute != null && pageAttribute.Template.StartsWith("/app/{app}", StringComparison.InvariantCultureIgnoreCase))
            {
                // Assert that the component inherits from BaseApplicationPage
                Assert.True(typeof(BaseApplicationPage).IsAssignableFrom(componentType),
                    $"{componentType.Name} should inherit from BaseApplicationPage");
            }
        }
    }

    /// <summary>
    /// Verify the <c href="HasAppHandler" /> authorization policy is applied to all application pages. So we cannot access an app that isn't ours.
    /// </summary>
    [Fact]
    public void BaseApplicationPage_Has_HasAppHandler_AuthorizationPolicy()
    {
        var type = typeof(BaseApplicationPage);
        var authorizeAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
        Assert.Equal(CustomPolicy.HasAppRole, authorizeAttribute?.Policy);
    }
}