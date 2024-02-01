using Passwordless.Common.Models.Authenticators;
using Passwordless.Service.Models;

namespace Passwordless.Service;

public interface IApplicationService
{
    Task SetFeaturesAsync(SetFeaturesDto features);
    Task<IEnumerable<ConfiguredAuthenticatorResponse>> ListConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request);
    Task AddAuthenticatorsAsync(AddAuthenticatorsRequest request);
    Task RemoveAuthenticatorsAsync(RemoveAuthenticatorsRequest request);
}