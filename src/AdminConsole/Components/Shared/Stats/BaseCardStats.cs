using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared.Stats;

public abstract class BaseCardStats : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    protected override void OnInitialized()
    {
        if (AdditionalAttributes.TryGetValue("class", out var classAttribute))
        {
            Class = $"{Class} {classAttribute}";
        }
    }

    public string Class { get; protected set; } = "mt-5 grid grid-cols-1 gap-5 sm:grid-cols-3";
}