using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Passwordless.Service.AuditLog;

public class AuditLoggerStorageFactory : IAuditLoggerStorageFactory
{
    private readonly IOptions<AuditLoggerStorageOptions> _options;
    private readonly IServiceProvider _provider;

    public AuditLoggerStorageFactory(IOptions<AuditLoggerStorageOptions> options, IServiceProvider provider)
    {
        _options = options;
        _provider = provider;
    }

    public IAuditLogStorage Create()
    {
        if (_options.Value.DatabaseStorage) return ActivatorUtilities.CreateInstance<AuditLoggerEfStorage>(_provider);

        throw new Exception("Invalid AuditLogger storage configuration");
    }
}