using System.ComponentModel;

namespace Passwordless.Common.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var attributes = (DescriptionAttribute[])enumValue
            .GetType()
            .GetField(enumValue.ToString())!
            .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
}