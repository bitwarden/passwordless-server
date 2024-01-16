using System.Text.Json;
using System.Text.Json.Serialization;

namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Serialization;

public static class ApexChartJsonSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}