using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Pages.App;

public abstract class BaseApplicationPage : ComponentBase
{
    /// <summary>
    /// The route parameter for the application id for application scoped pages.
    /// </summary>
    [Parameter]
    public required string App { get; set; }
}