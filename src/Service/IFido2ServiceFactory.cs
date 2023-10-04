using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IFido2ServiceFactory
{
    Task<IFido2Service> CreateAsync();
}

public class DefaultFido2ServiceFactory : IFido2ServiceFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;
    private readonly ITenantStorage _tenantStorage;
    private readonly ITokenService _tokenService;
    private readonly IEventLogger _eventLogger;
    private readonly IEventLogContext _eventLogContext;

    public DefaultFido2ServiceFactory(
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        ITenantStorage tenantStorage,
        ITokenService tokenService,
        IEventLogger eventLogger,
        IEventLogContext eventLogContext)
    {
        _loggerFactory = loggerFactory;
        _configuration = configuration;
        _tenantStorage = tenantStorage;
        _tokenService = tokenService;
        _eventLogger = eventLogger;
        _eventLogContext = eventLogContext;
    }

    public async Task<IFido2Service> CreateAsync()
    {
        var fidoService = await Fido2ServiceEndpoints.Create(
            _tenantStorage.Tenant,
            _loggerFactory.CreateLogger<Fido2ServiceEndpoints>(),
            _configuration,
            _tenantStorage,
            _tokenService,
            _eventLogger,
            _eventLogContext);
        return fidoService;
    }
}