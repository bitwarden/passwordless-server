namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

public class ApexChartOptions<TXValueType, TYValueType>
{
    public Title? Title { get; set; }

    public Chart? Chart { get; set; }

    public IEnumerable<Color>? Colors { get; set; } = new[] { Color.Default, Color.AmberOrange, Color.MelonRed, Color.MediumPurple };

    public IEnumerable<Series<TYValueType>>? Series { get; set; }

    public Xaxis<TXValueType>? Xaxis { get; set; }

    public Yaxis Yaxis { get; } = new();

    public NoData NoData { get; set; } = new();

    public Legend Legend { get; set; } = new();
}