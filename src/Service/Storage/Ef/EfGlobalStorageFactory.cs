using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.Storage.Ef;

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
        var timeProvider = _services.GetRequiredService<TimeProvider>();
        return new EfGlobalGlobalStorage(context, timeProvider);
    }
}