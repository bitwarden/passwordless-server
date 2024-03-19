using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared;

public partial class Alert : ComponentBase
{
    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? InputAttributes { get; set; }

    [Parameter]
    public required ContextualStyles Style { get; set; }

    public string? ContainerClass { get; private set; }

    public string? IconClass { get; private set; }

    public string? ContentClass { get; private set; }

    protected override void OnInitialized()
    {
        const string baseContainerClass = "rounded-md p-4";
        const string baseIconClass = "h-5 w-5";
        const string baseContentClass = "text-sm";

        switch (Style)
        {
            case ContextualStyles.Primary:
            case ContextualStyles.Info:
                ContainerClass = string.Join(" ", baseContainerClass, "bg-blue-50");
                IconClass = string.Join(" ", baseIconClass, "text-blue-400");
                ContentClass = string.Join(" ", baseContentClass, "text-blue-700");
                break;
            case ContextualStyles.Warning:
                ContainerClass = string.Join(" ", baseContainerClass, "bg-yellow-50");
                IconClass = string.Join(" ", baseIconClass, "text-yellow-400");
                ContentClass = string.Join(" ", baseContentClass, "text-yellow-700");
                break;
            case ContextualStyles.Danger:
                ContainerClass = string.Join(" ", baseContainerClass, "bg-red-50");
                IconClass = string.Join(" ", baseIconClass, "text-red-400");
                ContentClass = string.Join(" ", baseContentClass, "text-red-700");
                break;
            case ContextualStyles.Success:
                ContainerClass = string.Join(" ", baseContainerClass, "bg-green-50");
                IconClass = string.Join(" ", baseIconClass, "text-green-400");
                ContentClass = string.Join(" ", baseContentClass, "text-green-700");
                break;
            case ContextualStyles.Secondary:
                ContainerClass = string.Join(" ", baseContainerClass, "bg-gray-50");
                IconClass = string.Join(" ", baseIconClass, "text-gray-400");
                ContentClass = string.Join(" ", baseContentClass, "text-gray-700");
                break;
        }
    }
}