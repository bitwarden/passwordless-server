using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Passwordless.AdminConsole.Authorization;

namespace Passwordless.AdminConsole.Components.Pages.App;

[Authorize(CustomPolicy.HasAppRole)]
public abstract class BaseApplicationPage : ComponentBase
{
    /// <summary>
    /// The route parameter for the application id for application scoped pages.
    /// </summary>
    [Parameter]
    public required string App { get; set; }
}