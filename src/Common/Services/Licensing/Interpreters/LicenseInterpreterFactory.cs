using System.Collections.Immutable;
using System.Reflection;
using Passwordless.Common.Extensions;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.Interpreters;

public class LicenseInterpreterFactory : ILicenseInterpreterFactory
{
    private IReadOnlyDictionary<ushort, ILicenseInterpreter<BaseLicenseData>> _interpreters;
    private readonly ILogger<LicenseInterpreterFactory> _logger;

    public LicenseInterpreterFactory(ILogger<LicenseInterpreterFactory> logger)
    {
        _logger = logger;
        var assembly = Assembly.GetAssembly(typeof(LicenseInterpreterFactory))!;
        _interpreters = assembly.GetTypes()
            .Where(type =>
                type.IsClass &&
                type.GetInterfaces().Contains(typeof(ILicenseInterpreter<BaseLicenseData>)) &&
                Attribute.IsDefined(type, typeof(ManifestVersionAttribute)))
            .ToImmutableDictionary(
                k => k.GetCustomAttribute<ManifestVersionAttribute>().ManifestVersion,
                v => (ILicenseInterpreter<BaseLicenseData>)Activator.CreateInstance(v)!);
    }
    public ILicenseInterpreter<BaseLicenseData> Create(LicenseParameters parameters)
    {
        try
        {
            return _interpreters[parameters.ManifestVersion];
        }
        catch (KeyNotFoundException)
        {
            _logger.LogCritical("No license writer found for manifest version {ManifestVersion}.", parameters.ManifestVersion);
            throw;
        }
    }
}