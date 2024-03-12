using Passwordless.Common.EventLog.Enums;
using Passwordless.Common.EventLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.EventLog.Models;

public class ApplicationEvent : PerTenant, IEvent
{
    public string? ApiKeyId { get; set; }
    public Guid Id { get; init; }
    public DateTime PerformedAt { get; init; }
    public EventType EventType { get; init; }
    public string Message { get; init; }
    public Severity Severity { get; init; }
    public string PerformedBy { get; init; }
    public string Subject { get; init; }

    public AccountMetaInformation? Application { get; set; }
}