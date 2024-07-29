using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Passwordless.Common.Db.Converters;

/// <summary>
/// Converts an enum to a string using the EnumMemberAttribute value if present.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumToStringConverter<T> : ValueConverter<T, string> where T : struct, Enum
{
    public EnumToStringConverter() : base(
        v => GetEnumMemberValue(v),
        v => GetEnumFromMemberValue(v))
    {
    }

    private static string GetEnumMemberValue(T enumValue)
    {
        var enumType = typeof(T);
        var memberInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
        var enumMemberAttribute = memberInfo?.GetCustomAttribute<EnumMemberAttribute>();
        return enumMemberAttribute?.Value ?? enumValue.ToString();
    }

    private static T GetEnumFromMemberValue(string value)
    {
        var enumType = typeof(T);
        foreach (var memberInfo in enumType.GetMembers())
        {
            var enumMemberAttribute = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
            if (enumMemberAttribute?.Value == value)
            {
                return (T)Enum.Parse(enumType, memberInfo.Name);
            }
        }
        return (T)Enum.Parse(enumType, value);
    }
}