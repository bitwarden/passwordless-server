using Passwordless.AdminConsole.Models;

namespace Passwordless.AdminConsole.Services;

public interface IOrganizationFeatureService
{
    FeaturesContext GetOrganizationFeatures(int orgId);
}