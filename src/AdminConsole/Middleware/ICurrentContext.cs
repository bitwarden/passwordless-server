using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Middleware;

public interface ICurrentContext
{
    [MemberNotNullWhen(true, nameof(AppId))]
    [MemberNotNullWhen(true, nameof(ApplicationName))]
    [MemberNotNullWhen(true, nameof(ApiSecret))]
    [MemberNotNullWhen(true, nameof(ApiKey))]
    bool InAppContext { get; }
    string? AppId { get; }
    string? ApplicationName { get; }
    string? ApiSecret { get; }
    string? ApiKey { get; }
    bool IsPendingDelete { get; }
    string BillingPlan { get; }
    string? BillingPriceId { get; }
    ApplicationFeatureContext Features { get; }
    Organization? Organization { get; }
    int? OrgId { get; }
    OrganizationFeaturesContext OrganizationFeatures { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetApp(Application application);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetFeatures(ApplicationFeatureContext context);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("There should only be one caller of this method, you are probably not it.")]
    void SetOrganization(int organizationId, OrganizationFeaturesContext context, Organization organization);
}