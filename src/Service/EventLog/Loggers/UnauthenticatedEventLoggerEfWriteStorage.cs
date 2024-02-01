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
        var featuresContext = await _featureContextProvider.UseContext();

        if (featuresContext.IsInFeaturesContext && featuresContext.EventLoggingIsEnabled)
        {
            await base.FlushAsync();
        }
    }
}