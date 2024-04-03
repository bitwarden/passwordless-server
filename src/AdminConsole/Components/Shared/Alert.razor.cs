using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared;

public partial class Alert : ComponentBase
{
    private const string BaseContainerClass = "rounded-md p-4";
    private const string BaseIconClass = "h-5 w-5";
    private const string BaseContentClass = "text-sm";

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
        switch (Style)
        {
            case ContextualStyles.Primary:
            case ContextualStyles.Info:
                SetClasses("bg-blue-50", "text-blue-400", "text-blue-700");
                break;
            case ContextualStyles.Warning:
                SetClasses("bg-yellow-50", "text-yellow-400", "text-yellow-700");
                break;
            case ContextualStyles.Danger:
                SetClasses("bg-red-50", "text-red-400", "text-red-700");
                break;
            case ContextualStyles.Success:
                SetClasses("bg-green-50", "text-green-400", "text-green-700");
                break;
            case ContextualStyles.Secondary:
                SetClasses("bg-gray-50", "text-gray-400", "text-gray-700");
                break;
        }
    }

    private void SetClasses(string containerClass, string iconClass, string contentClass)
    {
        ContainerClass = string.Join(" ", BaseContainerClass, containerClass);
        IconClass = string.Join(" ", BaseIconClass, iconClass);
        ContentClass = string.Join(" ", BaseContentClass, contentClass);
    }
}