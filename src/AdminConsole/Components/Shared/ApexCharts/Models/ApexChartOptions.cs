namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

public class ApexChartOptions<TXValueType, TYValueType>
{
    public Title? Title { get; set; }

    public Chart? Chart { get; set; }

    public IEnumerable<Color>? Colors { get; set; }

    public IEnumerable<Series<TYValueType>>? Series { get; set; }

    public Xaxis<TXValueType>? Xaxis { get; set; }

    public NoData NoData { get; set; } = new();
}