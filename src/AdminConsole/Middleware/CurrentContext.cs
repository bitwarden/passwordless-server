using System.Diagnostics;
using Passwordless.AdminConsole.Models;
using Passwordless.Common.Models.Apps;

namespace Passwordless.AdminConsole.Middleware;

public class CurrentContext : ICurrentContext
{
#if DEBUG
    private bool _contextSet;
#endif

    public bool InAppContext { get; private set; }

    public string? AppId { get; private set; }

    public string? ApiSecret { get; private set; }

    public string? ApiKey { get; private set; }
    public bool IsFrozen { get; private set; }
    public FeaturesContext Features { get; private set; }
    public Organization? Organization { get; private set; }
    public int? OrgId { get; private set; }
    public FeaturesContext OrganizationFeatures { get; private set; } = new(false, 0, null, null, false);

    public void SetApp(Application application)
    {
#if DEBUG
        Debug.Assert(!_contextSet, "Context should only be set one time per lifetime.");
        _contextSet = true;
#endif
        InAppContext = true;
        AppId = application.Id;
        ApiSecret = application.ApiSecret;
        ApiKey = application.ApiKey;
        IsFrozen = application.DeleteAt.HasValue;
    }

    public void SetFeatures(FeaturesContext context)
    {
        Features = context;
    }

    public void SetOrganization(int organizationId, FeaturesContext featuresContext, Organization organization)
    {
        OrgId = organizationId;
        OrganizationFeatures = featuresContext;
        Organization = organization;
    }
}