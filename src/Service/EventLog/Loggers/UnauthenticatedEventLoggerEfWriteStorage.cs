using Passwordless.Service.EventLog.Mappings;
using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.EventLog.Loggers;

public class UnauthenticatedEventLoggerEfWriteStorage : EventLoggerEfWriteStorage
{
    public UnauthenticatedEventLoggerEfWriteStorage(DbGlobalContext storage,
        IEventLogContext eventLogContext)
        : base(storage, eventLogContext)
    {
    }

    public override void LogEvent(EventDto @event)
    {
        if (!HasEventLoggingFeature(@event.TenantId)) return;

        _storage.Add(@event.ToEvent());
        _storage.SaveChanges();
    }

    public override Task FlushAsync() => Task.CompletedTask;

    private bool HasEventLoggingFeature(string appId) =>
        _storage.AppFeatures.Any(x => x.Tenant == appId && x.EventLoggingIsEnabled);
}