using Microsoft.AspNetCore.Authentication;
using Passwordless.Common.Models;
using Passwordless.Service.Features;

namespace Passwordless.Service.EventLog.Models;

public interface IEventLogContext
{
    string TenantId { get; }
    IFeaturesContext Features { get; }
    string AbbreviatedKey { get; }
    DateTime PerformedAt { get; }
    bool Authenticated { get; }
    void SetContext(string tenantId, IFeaturesContext features, ApplicationPublicKey applicationPublicKey, bool authenticated);
    void SetContext(string tenantId, IFeaturesContext features, ApplicationSecretKey applicationPublicKey, bool authenticated);
    void SetContext(string tenantId, IFeaturesContext features, bool authenticated);
}

public class EventLogContext : IEventLogContext
{
    public EventLogContext(ISystemClock systemClock)
    {
        PerformedAt = systemClock.UtcNow.UtcDateTime;
    }

    public string TenantId { get; private set; } = string.Empty;
    public IFeaturesContext Features { get; private set; } = new NullFeaturesContext();
    public string AbbreviatedKey { get; private set; } = string.Empty;
    public DateTime PerformedAt { get; }
    public bool Authenticated { get; private set; }

    public void SetContext(string tenantId, IFeaturesContext features, ApplicationPublicKey applicationPublicKey, bool authenticated)
    {
        TenantId = tenantId;
        Features = features;
        AbbreviatedKey = applicationPublicKey.AbbreviatedValue;
        Authenticated = authenticated;
    }

    public void SetContext(string tenantId, IFeaturesContext features, ApplicationSecretKey applicationSecretKey, bool authenticated)
    {
        TenantId = tenantId;
        Features = features;
        AbbreviatedKey = applicationSecretKey.AbbreviatedValue;
        Authenticated = authenticated;
    }

    public void SetContext(string tenantId, IFeaturesContext features, bool authenticated)
    {
        TenantId = tenantId;
        Features = features;
        Authenticated = authenticated;
    }
}