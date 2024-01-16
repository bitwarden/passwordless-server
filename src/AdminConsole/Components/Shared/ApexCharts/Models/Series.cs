namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

public class Series<T>
{
    public string? Name { get; set; }

    public IEnumerable<T>? Data { get; set; }
}