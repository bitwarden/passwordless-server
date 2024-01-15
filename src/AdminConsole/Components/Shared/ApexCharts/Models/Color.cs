using System.Globalization;
using System.Text.Json.Serialization;
using Passwordless.AdminConsole.Components.Shared.ApexCharts.Serialization;
using Passwordless.AdminConsole.Components.Shared.ApexCharts.Validators;

namespace Passwordless.AdminConsole.Components.Shared.ApexCharts.Models;

[JsonConverter(typeof(ColorConverter))]
public class Color
{
    public static readonly Color Default = new("#2563eb");

    public Color(string hex)
    {
        if (!ColorValidator.IsValid(hex))
        {
            throw new ArgumentException("Invalid value", nameof(hex));
        }

        switch (hex.Length)
        {
            case 4:
            case 5:
                Red = byte.Parse(hex[1].ToString(), NumberStyles.HexNumber);
                Green = byte.Parse(hex[2].ToString(), NumberStyles.HexNumber);
                Blue = byte.Parse(hex[3].ToString(), NumberStyles.HexNumber);
                if (hex.Length == 5)
                {
                    Alpha = byte.Parse(hex[4].ToString(), NumberStyles.HexNumber);
                }
                break;
            case 7:
            case 9:
                Red = byte.Parse(hex[1..3], NumberStyles.HexNumber);
                Green = byte.Parse(hex[3..5], NumberStyles.HexNumber);
                Blue = byte.Parse(hex[5..7], NumberStyles.HexNumber);
                if (hex.Length == 9)
                {
                    Alpha = byte.Parse(hex[7..9], NumberStyles.HexNumber);
                }
                break;
        }
    }

    /// <summary>
    /// Create a color from RGB values.
    /// </summary>
    /// <param name="red">Red [0-255]</param>
    /// <param name="green">Green [0-255]</param>
    /// <param name="blue">Blue [0-255]</param>
    /// <param name="alpha">Alpha [0-255]</param>
    public Color(byte red, byte green, byte blue, byte? alpha = null)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    public byte Red { get; set; }

    public byte Green { get; set; }

    public byte Blue { get; set; }

    public byte? Alpha { get; set; }
}