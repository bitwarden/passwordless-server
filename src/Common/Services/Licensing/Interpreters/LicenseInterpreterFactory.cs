using System.Collections.Immutable;
using System.Reflection;
using Passwordless.Common.Extensions;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.Interpreters;

public class LicenseInterpreterFactory : ILicenseInterpreterFactory
{
    private readonly IReadOnlyDictionary<ushort, ILicenseInterpreter> _interpreters;
    private readonly ILogger<LicenseInterpreterFactory> _logger;

    public LicenseInterpreterFactory(ILogger<LicenseInterpreterFactory> logger)
    {
        _logger = logger;
        var assembly = Assembly.GetAssembly(typeof(LicenseInterpreterFactory))!;
        _interpreters = assembly.GetTypes()
            .Where(type =>
                type.IsClass &&
                type.GetInterfaces().Contains(typeof(ILicenseInterpreter)) &&
                Attribute.IsDefined(type, typeof(ManifestVersionAttribute)))
            .ToImmutableDictionary(
                k => k.GetCustomAttribute<ManifestVersionAttribute>().ManifestVersion,
                v => (ILicenseInterpreter)(Activator.CreateInstance(v)!));
    }

    public ILicenseInterpreter Create(LicenseParameters parameters)
    {
        try
        {
            return _interpreters[parameters.ManifestVersion];
        }
        catch (KeyNotFoundException)
        {
            _logger.LogError("No license interpreter found for manifest version {ManifestVersion}.", parameters.ManifestVersion);
            throw new ArgumentOutOfRangeException(nameof(parameters), $"No license interpreter found for manifest version '{parameters.ManifestVersion}'.");
        }
    }
}