using Microsoft.Extensions.DependencyInjection;

namespace Passwordless.Service.Storage.Ef;

public interface ITenantStorageFactory
{
    /// <summary>
    /// Use <see cref="ITenantStorage" /> instead when not calling from `ManagementKey` scope.
    /// <see cref="ITenantStorage" /> uses the `accountName` claim in <see cref="TenantProvider" /> to determine the tenant when an authorization is successful.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    [Obsolete("Use injected `ITenantStorage` instead when not calling from `ManagementKey` scope.")]
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
        var context = ActivatorUtilities.CreateInstance<TContext>(_services, new ManualTenantProvider(appId));
        return new EfTenantStorage(context, _timeProvider, new ManualTenantProvider(appId));
    }
}