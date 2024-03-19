using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared.Icons.Alerts;

public class BaseAlertIcon : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public ContextualStyles? Style { get; set; }

    public string? StyleClass => Style switch
    {
        ContextualStyles.Primary => "fill-blue-400",
        ContextualStyles.Info => "fill-blue-400",
        ContextualStyles.Warning => "fill-yellow-400",
        ContextualStyles.Danger => "fill-red-400",
        ContextualStyles.Secondary => "fill-gray-400",
        ContextualStyles.Success => "fill-green-400",
        _ => null
    };

    protected override void OnInitialized()
    {
        if (AdditionalAttributes == null)
        {
            AdditionalAttributes = new Dictionary<string, object>();
        }

        if (Style.HasValue)
        {
            if (AdditionalAttributes.ContainsKey("class"))
            {
                var currentClasses = AdditionalAttributes["class"].ToString();
                AdditionalAttributes["class"] = $"{currentClasses} {StyleClass}";
            }
            else
            {
                AdditionalAttributes.Add("class", StyleClass);
            }
        }
    }
}