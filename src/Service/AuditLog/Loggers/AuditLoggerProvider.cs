using Microsoft.Extensions.DependencyInjection;
using Passwordless.Service.Features;

namespace Passwordless.Service.AuditLog.Loggers;

public class AuditLoggerProvider
{
    private readonly IFeatureContextProvider _featureContextProvider;
    private readonly IServiceProvider _serviceProvider;

    public AuditLoggerProvider(IFeatureContextProvider featureContextProvider, IServiceProvider serviceProvider)
    {
        _featureContextProvider = featureContextProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<IAuditLogger> Create() =>
        (await _featureContextProvider.UseContext()).AuditLoggingIsEnabled
            ? _serviceProvider.GetService<AuditLoggerEfStorage>()
            : NoOpAuditLogger.Instance;
}