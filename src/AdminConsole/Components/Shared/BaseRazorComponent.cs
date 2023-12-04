using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared;

public abstract class BaseRazorComponent : ComponentBase
{
    [Parameter]
    public string? Class { get; set; }
}