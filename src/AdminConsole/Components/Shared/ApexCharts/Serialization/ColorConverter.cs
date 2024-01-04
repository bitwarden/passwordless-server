using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Serialization;

public sealed class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        var hexBuilder = new StringBuilder($"#{value.Red:X2}{value.Green:X2}{value.Blue:X2}");

        if (value.Alpha.HasValue)
        {
            hexBuilder.Append($"{value.Alpha.Value:X2}");
        }

        writer.WriteStringValue(hexBuilder.ToString());
    }
}