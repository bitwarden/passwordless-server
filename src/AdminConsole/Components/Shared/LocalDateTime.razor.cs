using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Passwordless.AdminConsole.Components.Shared;

/// <summary>
/// Renders a <see cref="DateTime" /> in the user's local time zone. 
/// </summary>
public partial class LocalDateTime : ComponentBase
{
    public string Id { get; } = Guid.NewGuid().ToString();

    [Parameter]
    public required DateTime Value { get; set; }

    [Parameter]
    public string? DateFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffZ";

    private string? _isoValue;

    protected override void OnInitialized()
    {
        _isoValue = Value.ToString(DateFormat, CultureInfo.InvariantCulture);
    }
}