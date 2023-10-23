using Passwordless.Common.EventLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.EventLog.Models;

public class ApplicationEvent : Event
{
    public string TenantId { get; init; }
    public string ApiKeyId { get; init; }
    public AccountMetaInformation Application { get; init; }
}