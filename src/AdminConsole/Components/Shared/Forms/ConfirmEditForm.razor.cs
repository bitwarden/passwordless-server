using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Passwordless.AdminConsole.Components.Shared.Forms;

/// <summary>
/// This form component will intercept any submitted forms and display a confirmation dialog before submitting the form to the server.
/// </summary>
public partial class ConfirmEditForm : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? FormName { get; set; }

    [Parameter]
    public object? Model { get; set; }

    [Parameter]
    public EventCallback<EditContext> OnSubmit { get; set; }

    [Parameter]
    public EventCallback<EditContext> OnValidSubmit { get; set; }
}