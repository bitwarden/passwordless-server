using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Authenticators;

namespace Passwordless.Service;

public interface IApplicationService
{
    Task SetFeaturesAsync(SetFeaturesRequest features);
    Task<IEnumerable<ConfiguredAuthenticatorResponse>> ListConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request);
    Task AddAuthenticatorsAsync(AddAuthenticatorsRequest request);
    Task RemoveAuthenticatorsAsync(RemoveAuthenticatorsRequest request);
}