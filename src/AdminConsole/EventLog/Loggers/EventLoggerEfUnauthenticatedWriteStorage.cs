using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace Passwordless.AdminConsole.EventLog.Loggers;

public class EventLoggerEfUnauthenticatedWriteStorage : EventLoggerEfWriteStorage
{
    private readonly OrganizationFeatureService _organizationFeatureService;

    public EventLoggerEfUnauthenticatedWriteStorage(ConsoleDbContext context, OrganizationFeatureService organizationFeatureService) : base(context)
    {
        _organizationFeatureService = organizationFeatureService;
    }

    public override void LogEvent(OrganizationEventDto @event)
    {
        if (HasEventLoggingEnabled(@event.OrganizationId))
        {
            base.LogEvent(@event);
        }
    }

    private bool HasEventLoggingEnabled(int organizationId)
    {
        return _organizationFeatureService.GetOrganizationFeatures(organizationId).EventLoggingIsEnabled;
    }
}