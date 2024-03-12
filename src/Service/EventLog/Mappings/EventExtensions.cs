using Passwordless.Service.EventLog.Models;

namespace Passwordless.Service.EventLog.Mappings;

public static class EventExtensions
{
    public static EventResponse ToEvent(this ApplicationEvent dbEvent) => new
    (
        dbEvent.PerformedAt,
        dbEvent.Message,
        dbEvent.PerformedBy,
        dbEvent.Tenant,
        dbEvent.EventType,
        dbEvent.Severity,
        dbEvent.Subject,
        dbEvent.ApiKeyId
    );

    public static ApplicationEvent ToEvent(this EventDto eventDto) => new()
    {
        Id = Guid.NewGuid(),
        PerformedAt = eventDto.PerformedAt,
        EventType = eventDto.EventType,
        Message = eventDto.Message,
        Severity = eventDto.Severity,
        PerformedBy = eventDto.PerformedBy,
        Subject = eventDto.Subject,
        Tenant = eventDto.TenantId,
        ApiKeyId = eventDto.ApiKeyId
    };
}