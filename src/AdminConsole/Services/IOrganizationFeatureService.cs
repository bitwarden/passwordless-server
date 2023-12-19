using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public interface IOrganizationFeatureService
{
    OrganizationFeaturesContext GetOrganizationFeatures(int orgId);
}