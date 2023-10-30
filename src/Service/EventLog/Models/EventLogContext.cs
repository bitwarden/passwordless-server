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
    bool IsAuthenticated { get; }
    void SetContext(string tenantId, IFeaturesContext features, ApplicationPublicKey applicationPublicKey, bool isAuthenticated);
    void SetContext(string tenantId, IFeaturesContext features, ApplicationSecretKey applicationPublicKey, bool isAuthenticated);
    void SetContext(string tenantId, IFeaturesContext features, bool isAuthenticated);
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
    public bool IsAuthenticated { get; private set; }

    public void SetContext(string tenantId, IFeaturesContext features, ApplicationPublicKey applicationPublicKey, bool isAuthenticated)
    {
        TenantId = tenantId;
        Features = features;
        AbbreviatedKey = applicationPublicKey.AbbreviatedValue;
        IsAuthenticated = isAuthenticated;
    }

    public void SetContext(string tenantId, IFeaturesContext features, ApplicationSecretKey applicationSecretKey, bool isAuthenticated)
    {
        TenantId = tenantId;
        Features = features;
        AbbreviatedKey = applicationSecretKey.AbbreviatedValue;
        IsAuthenticated = isAuthenticated;
    }

    public void SetContext(string tenantId, IFeaturesContext features, bool isAuthenticated)
    {
        TenantId = tenantId;
        Features = features;
        IsAuthenticated = isAuthenticated;
    }
}