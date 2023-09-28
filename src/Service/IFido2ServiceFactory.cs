using Microsoft.Extensions.Logging;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.AuditLog.Models;
using Passwordless.Service.Features;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IFido2ServiceFactory
{
    Task<IFido2Service> CreateAsync();
}

public class DefaultFido2ServiceFactory : IFido2ServiceFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITenantStorage _tenantStorage;
    private readonly ITokenService _tokenService;
    private readonly IAuditLogger _auditLogger;
    private readonly IAuditLogContext _auditLogContext;
    private readonly IFeatureContextProvider _featureContextProvider;

    public DefaultFido2ServiceFactory(
        ILoggerFactory loggerFactory,
        ITenantStorage tenantStorage,
        ITokenService tokenService,
        IAuditLogger auditLogger,
        IAuditLogContext auditLogContext,
        IFeatureContextProvider featureContextProvider)
    {
        _loggerFactory = loggerFactory;
        _tenantStorage = tenantStorage;
        _tokenService = tokenService;
        _auditLogger = auditLogger;
        _auditLogContext = auditLogContext;
        _featureContextProvider = featureContextProvider;
    }

    public async Task<IFido2Service> CreateAsync()
    {
        var fidoService = await Fido2Service.Create(
            _tenantStorage.Tenant,
            _loggerFactory.CreateLogger<Fido2Service>(),
            _tenantStorage,
            _tokenService,
            _featureContextProvider,
            _auditLogger,
            _auditLogContext);
        return fidoService;
    }
}