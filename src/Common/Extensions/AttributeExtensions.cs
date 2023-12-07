using System.Reflection;

namespace Passwordless.Common.Extensions;

public static class AttributeExtensions
{
    /// <summary>
    /// Gets the custom attribute of the specified type.
    /// </summary>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T GetCustomAttribute<T>(this Type type) where T : Attribute
    {
        var attribute = type.GetCustomAttribute(typeof(T));
        if (attribute == null)
        {
            throw new Exception($"Type {type.Name} does not have a {typeof(T).Name} attribute.");
        }

        return (T)attribute;
    }
}