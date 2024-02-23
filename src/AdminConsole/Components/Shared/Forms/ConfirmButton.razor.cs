using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared.Forms;

/// <summary>
/// This button component allows to customize the confirmation dialog that is displayed when the button is clicked.
/// </summary>
public partial class ConfirmButton : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The title in the confirmation dialog.
    /// </summary>
    [Parameter]
    public string? ConfirmTitle { get; set; }

    /// <summary>
    /// The description or message in the confirmation dialog.
    /// </summary>
    [Parameter]
    public string? ConfirmDescription { get; set; }

    /// <summary>
    /// The text displayed on the confirmation button.
    /// </summary>
    [Parameter]
    public string? ConfirmButtonText { get; set; }

    /// <summary>
    /// The text displayed on the cancellation button
    /// </summary>
    [Parameter]
    public string? CancelButtonText { get; set; }

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(ConfirmTitle))
        {
            AdditionalAttributes["confirm-title"] = ConfirmTitle;
        }

        if (!string.IsNullOrWhiteSpace(ConfirmDescription))
        {
            AdditionalAttributes["confirm-description"] = ConfirmDescription;
        }

        if (!string.IsNullOrWhiteSpace(ConfirmButtonText))
        {
            AdditionalAttributes["confirm-button-text"] = ConfirmButtonText;
        }

        if (!string.IsNullOrWhiteSpace(CancelButtonText))
        {
            AdditionalAttributes["cancel-button-text"] = CancelButtonText;
        }
    }
}