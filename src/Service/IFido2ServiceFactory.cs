using Microsoft.Extensions.Logging;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IFido2ServiceFactory
{
    Task<IFido2Service> CreateAsync(CancellationToken cancellationToken);
}

public class DefaultFido2ServiceFactory : IFido2ServiceFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITenantStorage _tenantStorage;
    private readonly ITokenService _tokenService;
    private readonly IEventLogger _eventLogger;

    public DefaultFido2ServiceFactory(
        ILoggerFactory loggerFactory,
        ITenantStorage tenantStorage,
        ITokenService tokenService,
        IEventLogger eventLogger)
    {
        _loggerFactory = loggerFactory;
        _tenantStorage = tenantStorage;
        _tokenService = tokenService;
        _eventLogger = eventLogger;
    }

    public async Task<IFido2Service> CreateAsync(CancellationToken cancellationToken)
    {
        var fidoService = await Fido2ServiceEndpoints.Create(
            _tenantStorage.Tenant,
            _loggerFactory.CreateLogger<Fido2ServiceEndpoints>(),
            _tenantStorage,
            _tokenService,
            _eventLogger,
            cancellationToken);
        return fidoService;
    }
}