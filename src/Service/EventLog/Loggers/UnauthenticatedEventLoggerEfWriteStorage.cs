using Passwordless.Service.EventLog.Models;
using Passwordless.Service.Features;
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
        if (!_storage.AppFeatures.Any(x => x.Tenant == @event.TenantId && x.EventLoggingIsEnabled))
        {
            return;
        }

        _storage.Add(@event);
        _storage.SaveChanges();
    }

    public override Task FlushAsync() => Task.CompletedTask;
}