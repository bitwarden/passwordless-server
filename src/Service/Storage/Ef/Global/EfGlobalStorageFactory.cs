using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.Storage.Ef.Global;

public class EfGlobalStorageFactory<TContext> : IGlobalStorageFactory
    where TContext : DbGlobalContext
{
    private readonly IServiceProvider _services;

    public EfGlobalStorageFactory(IServiceProvider services)
    {
        _services = services;
    }

    /**
     * If you need more than the tenant scope.
     */
    public IGlobalStorage Create()
    {
        var context = ActivatorUtilities.CreateInstance<TContext>(_services);
        var systemClock = _services.GetRequiredService<ISystemClock>();
        return new EfGlobalGlobalStorage(context, systemClock);
    }
}