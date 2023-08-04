using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.Storage.Ef;

public class EfStorageFactory<TContext> : IStorageFactory
    where TContext : DbTenantContext
{
    private readonly IServiceProvider _services;

    public EfStorageFactory(IServiceProvider services)
    {
        _services = services;
    }

    /**
     * If you need more than the tenant scope.
     */
    public IStorage Create()
    {
        var context = ActivatorUtilities.CreateInstance<TContext>(_services, new NoTenantProvider());
        var systemClock = _services.GetRequiredService<ISystemClock>();
        return new EfStorage(context, systemClock);
    }
}