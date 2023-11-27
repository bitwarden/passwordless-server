using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.Storage.Ef;

public interface ITenantStorageFactory
{
    ITenantStorage Create(string appId);
}

public class EfTenantStorageFactory<TContext> : ITenantStorageFactory
    where TContext : DbTenantContext
{
    private readonly IServiceProvider _services;
    private readonly TimeProvider _timeProvider;

    public EfTenantStorageFactory(IServiceProvider services, TimeProvider timeProvider)
    {
        _services = services;
        _timeProvider = timeProvider;
    }

    public ITenantStorage Create(string appId)
    {
        var context = ActivatorUtilities.CreateInstance<TContext>(_services, new ManualTenantProvider(appId, _timeProvider));
        return new EfTenantStorage(context);
    }
}