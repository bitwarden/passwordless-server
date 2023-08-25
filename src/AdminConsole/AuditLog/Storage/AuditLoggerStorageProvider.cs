using System.Configuration;
using Microsoft.Extensions.Options;

namespace Passwordless.AdminConsole.AuditLog.Storage;

public class AuditLoggerStorageProvider : IAuditLoggerStorageProvider
{
    private readonly IOptions<AuditLoggerStorageOptions> _options;
    private readonly IAuditLoggerEfStorage _efStorage;

    public AuditLoggerStorageProvider(IOptions<AuditLoggerStorageOptions> options, IAuditLoggerEfStorage efStorage)
    {
        _options = options;
        _efStorage = efStorage;
    }

    public IAuditLoggerStorage Create()
    {
        if (_options.Value.DatabaseStorage) return _efStorage;

        throw new ConfigurationErrorsException("Storage option not supported.");
    }
}