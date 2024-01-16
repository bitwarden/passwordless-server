using System.Text.Json;
using Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;
using Passwordless.AdminConsole.Components.Shared.ApexCharts.Serialization;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.ApexCharts.Serialization;

public class ApexChartJsonSerializerTests
{
    [Fact]
    public void Enums_Are_Serialized_As_Strings()
    {
        // Arrange
        var expected = "\"line\"";

        // Act
        var actual = JsonSerializer.Serialize(ChartTypes.Line, ApexChartJsonSerializer.Options);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Color_Is_Serialized_As_Hex()
    {
        // Arrange
        var expected = "\"#FF0000\"";

        // Act
        var actual = JsonSerializer.Serialize(new Color("#FF0000"), ApexChartJsonSerializer.Options);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Properties_With_Null_Values_Are_Not_Serialized()
    {
        // Arrange
        var expected = "{}";

        // Act
        var actual = JsonSerializer.Serialize(new ApexChartOptions<string, int>(), ApexChartJsonSerializer.Options);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Properties_Are_Serialized_With_CamelCase()
    {
        // Arrange
        var expected = "{\"chart\":{\"type\":\"line\"}}";

        // Act
        var actual = JsonSerializer.Serialize(new ApexChartOptions<string, int>
        {
            Chart = new Chart
            {
                Type = ChartTypes.Line
            }
        }, ApexChartJsonSerializer.Options);

        // Assert
        Assert.Equal(expected, actual);
    }
}