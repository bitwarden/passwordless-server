using Passwordless.Common.Models;
using Passwordless.Service.Features;

namespace Passwordless.Service.AuditLog.Models;

public interface IAuditLogContext
{
    string TenantId { get; }
    IFeaturesContext Features { get; }
    string AbbreviatedKey { get; }
    void SetContext(string tenantId, IFeaturesContext features, ApplicationPublicKey applicationPublicKey);
    void SetContext(string tenantId, IFeaturesContext features, ApplicationSecretKey applicationSecretKeyKey);
    void SetContext(string tenantId, IFeaturesContext features);
}

public class AuditLogContext : IAuditLogContext
{
    public string TenantId { get; private set; } = string.Empty;
    public IFeaturesContext Features { get; private set; } = new NullFeaturesContext();
    public string AbbreviatedKey { get; private set; } = string.Empty;

    public void SetContext(string tenantId, IFeaturesContext features, ApplicationPublicKey applicationPublicKey)
    {
        TenantId = tenantId;
        Features = features;
        AbbreviatedKey = applicationPublicKey.AbbreviatedValue;
    }

    public void SetContext(string tenantId, IFeaturesContext features, ApplicationSecretKey applicationSecretKey)
    {
        TenantId = tenantId;
        Features = features;
        AbbreviatedKey = applicationSecretKey.AbbreviatedValue;
    }

    public void SetContext(string tenantId, IFeaturesContext features)
    {
        TenantId = tenantId;
        Features = features;
    }
}