using Microsoft.Extensions.Logging;
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
    private readonly IFeatureContextProvider _featureContextProvider;

    public DefaultFido2ServiceFactory(
        ILoggerFactory loggerFactory,
        ITenantStorage tenantStorage,
        ITokenService tokenService,
        IFeatureContextProvider featureContextProvider)
    {
        _loggerFactory = loggerFactory;
        _tenantStorage = tenantStorage;
        _tokenService = tokenService;
        _featureContextProvider = featureContextProvider;
    }

    public async Task<IFido2Service> CreateAsync()
    {
        var fidoService = await Fido2Service.Create(
            _tenantStorage.Tenant,
            _loggerFactory.CreateLogger<Fido2Service>(),
            _tenantStorage,
            _tokenService,
            _featureContextProvider);
        return fidoService;
    }
}