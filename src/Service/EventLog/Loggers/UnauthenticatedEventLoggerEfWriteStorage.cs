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
        if ((await _featureContextProvider.UseContext()).EventLoggingIsEnabled)
        {
            await base.FlushAsync();
        }
    }
}