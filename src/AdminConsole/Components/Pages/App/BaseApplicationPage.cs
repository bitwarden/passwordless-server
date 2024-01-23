using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Pages.App;

public abstract class BaseApplicationPage : ComponentBase
{
    [Parameter]
    public required string App { get; set; }
}