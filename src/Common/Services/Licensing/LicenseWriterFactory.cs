using System.Collections.Immutable;
using System.Reflection;
using Passwordless.Common.Extensions;

namespace Passwordless.Common.Services.Licensing;

public class LicenseWriterFactory : ILicenseWriterFactory
{
    private readonly IReadOnlyDictionary<ushort, ILicenseWriter> _writers;
    private readonly ILogger<LicenseWriterFactory> _logger;

    public LicenseWriterFactory(ILogger<LicenseWriterFactory> logger)
    {
        _logger = logger;
    }
    
    public LicenseWriterFactory()
    {
        var assembly = Assembly.GetAssembly(typeof(LicenseWriterFactory))!;
        _writers = assembly.GetTypes()
            .Where(type =>
                type.IsClass &&
                type.GetInterfaces().Contains(typeof(ILicenseWriter)) &&
                Attribute.IsDefined(type, typeof(ManifestVersionAttribute)))
            .ToImmutableDictionary(
                k => k.GetCustomAttribute<ManifestVersionAttribute>().ManifestVersion,
                v => (ILicenseWriter)Activator.CreateInstance(v)!);
    }
    
    public ILicenseWriter Create(ushort manifestVersion)
    {
        try
        {
            return _writers[manifestVersion];
        }
        catch (KeyNotFoundException)
        {
            _logger.LogCritical($"No license writer found for manifest version {manifestVersion}.");
            throw;
        }
    }

    public IReadOnlyDictionary<ushort, ILicenseWriter> Writers => _writers;
}