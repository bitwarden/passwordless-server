using Passwordless.Common.EventLog.Models;
using Passwordless.Service.Models;

namespace Passwordless.Service.EventLog.Models;

public class ApplicationEvent : Event
{
    public required string TenantId { get; set; }
    public string? ApiKeyId { get; set; }

    public AccountMetaInformation? Application { get; set; }
}