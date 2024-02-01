using Passwordless.Common.Models.Authenticators;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public class ApplicationService : IApplicationService
{
    private readonly ITenantStorage _storage;
    public ApplicationService(ITenantStorage storage)
    {
        _storage = storage;
    }

    public async Task SetFeaturesAsync(SetFeaturesDto features)
    {
        if (features == null)
        {
            throw new ApiException("No 'body' or 'parameters' were passed.", 400);
        }

        await _storage.SetFeaturesAsync(features);
    }

    public async Task<IEnumerable<ConfiguredAuthenticatorResponse>> ListConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request)
    {
        var entities = await _storage.GetAuthenticatorsAsync(request.IsAllowed);
        return entities.Select(x => new ConfiguredAuthenticatorResponse(x.AaGuid, x.CreatedAt)).ToList();
    }

    public Task AddAuthenticatorsAsync(AddAuthenticatorsRequest request)
    {
        return _storage.AddAuthenticatorsAsync(request.AaGuids, request.IsAllowed);
    }

    public Task RemoveAuthenticatorsAsync(RemoveAuthenticatorsRequest request)
    {
        return _storage.RemoveAuthenticatorsAsync(request.AaGuids);
    }
}