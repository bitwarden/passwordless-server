using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Features;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.EventLog.Loggers;

public class UnauthenticatedEventLoggerEfWriteStorage : EventLoggerEfWriteStorage
{
    private readonly IFeatureContextProvider _featureContextProvider;

    public UnauthenticatedEventLoggerEfWriteStorage(
        DbGlobalContext storage,
        IEventLogContext eventLogContext,
        IFeatureContextProvider featureContextProvider,
        EventCache eventCache) : base(storage, eventLogContext, eventCache)
    {
        _featureContextProvider = featureContextProvider;
    }

    public async override Task FlushAsync()
    {
        if (await HasFeature())
        {
            await base.FlushAsync();
        }
    }

    private async Task<bool> HasFeature()
    {
        var appId = _eventCache.GetEvents().FirstOrDefault()?.TenantId ?? string.Empty;

        return (await _featureContextProvider.UseContext(appId)).EventLoggingIsEnabled;
    }
}