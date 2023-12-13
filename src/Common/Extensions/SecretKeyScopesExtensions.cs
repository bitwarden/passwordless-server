using System.ComponentModel;
using Passwordless.Common.Constants;

namespace Passwordless.Common.Extensions;

public static class SecretKeyScopesExtensions
{
    public static string GetValue(this SecretKeyScopes enumValue)
    {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])enumValue
            .GetType()
            .GetField(enumValue.ToString())!
            .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Any() ? attributes[0].Description : string.Empty;
    }

    public static SecretKeyScopes AsSecretKeyScope(this string value)
    {
        foreach (SecretKeyScopes enumValue in Enum.GetValues(typeof(SecretKeyScopes)))
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