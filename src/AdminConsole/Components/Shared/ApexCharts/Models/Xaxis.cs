namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

public class Xaxis<TXValueType>
{
    public XAxisTypes? Type { get; set; }

    public IEnumerable<TXValueType>? Categories { get; set; }
}