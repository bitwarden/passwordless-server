using Microsoft.Extensions.Logging;
using Passwordless.Service.AuditLog.Loggers;
using Passwordless.Service.AuditLog.Models;
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

    public DefaultFido2ServiceFactory(
        ILoggerFactory loggerFactory,
        ITenantStorage tenantStorage,
        ITokenService tokenService,
        IAuditLogger auditLogger,
        IAuditLogContext auditLogContext)
    {
        _loggerFactory = loggerFactory;
        _tenantStorage = tenantStorage;
        _tokenService = tokenService;
        _auditLogger = auditLogger;
        _auditLogContext = auditLogContext;
    }

    public async Task<IFido2Service> CreateAsync()
    {
        var fidoService = await Fido2ServiceEndpoints.Create(
            _tenantStorage.Tenant,
            _loggerFactory.CreateLogger<Fido2ServiceEndpoints>(),
            _tenantStorage,
            _tokenService,
            _auditLogger,
            _auditLogContext);
        return fidoService;
    }
}