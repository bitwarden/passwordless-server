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
}