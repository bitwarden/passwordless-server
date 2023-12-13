using System.ComponentModel;
using Passwordless.Common.Constants;

namespace Passwordless.Common.Extensions;

public static class PublicKeyScopesExtensions
{
    public static string GetValue(this PublicKeyScopes enumValue)
    {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])enumValue
            .GetType()
            .GetField(enumValue.ToString())!
            .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

    public static PublicKeyScopes AsPublicKeyScope(this string value)
    {
        foreach (PublicKeyScopes enumValue in Enum.GetValues(typeof(PublicKeyScopes)))
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])enumValue
                .GetType()
                .GetField(enumValue.ToString())!
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            var potentialMatch = attributes.Any() ? attributes[0].Description : string.Empty;

            if (potentialMatch == value)
            {
                return enumValue;
            }
        }

        throw new ArgumentException("Invalid scope.", nameof(value));
    }
}