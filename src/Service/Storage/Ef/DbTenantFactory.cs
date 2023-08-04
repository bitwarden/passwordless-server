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

    public EfTenantStorageFactory(IServiceProvider services)
    {
        _services = services;
    }

    public ITenantStorage Create(string appId)
    {
        var context = ActivatorUtilities.CreateInstance<TContext>(_services, new ManualTenantProvider(appId));
        return new EfTenantStorage(context);
    }
}