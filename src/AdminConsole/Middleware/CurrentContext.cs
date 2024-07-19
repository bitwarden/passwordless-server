using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Middleware;

public class CurrentContext : ICurrentContext
{
    public bool InAppContext { get; private set; }

    public string? AppId { get; private set; }

    public string? ApiSecret { get; private set; }

    public string? ApiKey { get; private set; }
    public bool IsPendingDelete { get; private set; }
    public string BillingPlan { get; private set; }
    public string? BillingPriceId { get; private set; }
    public ApplicationFeatureContext Features { get; private set; }
    public Organization? Organization { get; private set; }
    public int? OrgId { get; private set; }
    public OrganizationFeaturesContext OrganizationFeatures { get; private set; } = new(false, 0);

    public void SetApp(Application application)
    {
        InAppContext = true;
        AppId = application.Id;
        ApiSecret = application.ApiSecret;
        ApiKey = application.ApiKey;
        IsPendingDelete = application.DeleteAt.HasValue;
        BillingPlan = application.BillingPlan;
        BillingPriceId = application.BillingPriceId;
    }

    public void SetFeatures(ApplicationFeatureContext context)
    {
        Features = context;
    }

    public void SetOrganization(int organizationId, OrganizationFeaturesContext organizationFeaturesContext, Organization organization)
    {
        OrgId = organizationId;
        OrganizationFeatures = organizationFeaturesContext;
        Organization = organization;
    }
}