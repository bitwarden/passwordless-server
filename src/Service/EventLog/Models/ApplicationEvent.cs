using Passwordless.Common.EventLog.Models;

namespace Passwordless.Service.EventLog.Models;

public class ApplicationEvent : Event
{
    public string TenantId { get; init; }
    public string ApiKeyId { get; init; }
}