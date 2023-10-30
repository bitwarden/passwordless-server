using System.Reflection;

namespace Passwordless.Common.Extensions;

public static class AssemblyExtensions
{
    public static string? GetInformationalVersion(this Assembly assembly) =>
        assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}