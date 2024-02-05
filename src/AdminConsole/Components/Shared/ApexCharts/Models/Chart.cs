namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

public class Chart
{
    public ChartTypes Type { get; set; }

    public string? Height { get; set; }

    public bool? Stacked { get; set; }

    public string? StackType { get; set; }
}