namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

public class Legend
{
    public bool Show { get; set; } = true;

    public bool ShowForSingleSeries { get; set; } = true;

    public bool ShowForNullSeries { get; set; } = true;

    public bool ShowForZeroSeries { get; set; } = true;
}