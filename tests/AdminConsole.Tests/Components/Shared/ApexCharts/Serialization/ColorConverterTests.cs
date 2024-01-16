using System.Text.Json;
using Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.ApexCharts.Serialization;

public class ColorConverterTests
{
    [Fact]
    public void Write_ConvertsColorToHexFormat()
    {
        // Arrange
        var color = new Color(255, 0, 0, 127);

        // Act
        var actual = JsonSerializer.Serialize(color);

        // Assert
        Assert.Equal("\"#FF00007F\"", actual);
    }

    [Fact]
    public void Write_ConvertsColorWithoutAlphaToHexFormat()
    {
        // Arrange
        var color = new Color(255, 0, 0);

        // Act
        var actual = JsonSerializer.Serialize(color);

        // Assert
        Assert.Equal("\"#FF0000\"", actual);
    }
}