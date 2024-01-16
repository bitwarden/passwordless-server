using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.Storage.Ef;

public interface ITenantStorageFactory
{
    ITenantStorage Create();
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

    public ITenantStorage Create()
    {
        var tenantProvider = _services.GetRequiredService<ITenantProvider>();
        var context = ActivatorUtilities.CreateInstance<TContext>(_services, tenantProvider);

        return new EfTenantStorage(context, _timeProvider);
    }

    public ITenantStorage Create(string appId)
    {
        var context = ActivatorUtilities.CreateInstance<TContext>(_services, new ManualTenantProvider(appId));
        return new EfTenantStorage(context, _timeProvider);
    }
}