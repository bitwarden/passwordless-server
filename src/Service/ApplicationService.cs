using Passwordless.Common.Models.Apps;
using Passwordless.Common.Models.Authenticators;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.MDS;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public class ApplicationService : IApplicationService
{
    private readonly ITenantStorage _storage;
    private readonly IEventLogger _eventLogger;
    private readonly IMetaDataService _metaDataService;

    public ApplicationService(
        ITenantStorage storage,
        IEventLogger eventLogger,
        IMetaDataService metaDataService)
    {
        _storage = storage;
        _eventLogger = eventLogger;
        _metaDataService = metaDataService;
    }

    public async Task SetFeaturesAsync(SetFeaturesRequest features)
    {
        if (features == null)
        {
            throw new ApiException("No 'body' or 'parameters' were passed.", 400);
        }


        await _storage.SetFeaturesAsync(features);

        // Event logging
        if (features.EnableManuallyGeneratedAuthenticationTokens == true) _eventLogger.LogGenerateSignInTokenEndpointEnabled(features.PerformedBy);
        if (features.EnableManuallyGeneratedAuthenticationTokens == false) _eventLogger.LogGenerateSignInTokenEndpointDisabled(features.PerformedBy);
        if (features.EnableMagicLinks == true) _eventLogger.LogMagicLinksEnabled(features.PerformedBy);
        if (features.EnableMagicLinks == false) _eventLogger.LogMagicLinksDisabled(features.PerformedBy);
    }

    public async Task<IEnumerable<ConfiguredAuthenticatorResponse>> ListConfiguredAuthenticatorsAsync(ConfiguredAuthenticatorRequest request)
    {
        var entities = await _storage.GetAuthenticatorsAsync(request.IsAllowed);
        return entities.Select(x => new ConfiguredAuthenticatorResponse(x.AaGuid, x.CreatedAt)).ToList();
    }

    public async Task AddAuthenticatorsAsync(AddAuthenticatorsRequest request)
    {
        if (!(await _metaDataService.ExistsAsync(request.AaGuids)))
        {
            throw new ApiException("One or more authenticators do not exist in the FIDO2 MDS.", 400);
        }

        await _storage.AddAuthenticatorsAsync(request.AaGuids, request.IsAllowed);
    }

    public Task RemoveAuthenticatorsAsync(RemoveAuthenticatorsRequest request)
    {
        return _storage.RemoveAuthenticatorsAsync(request.AaGuids);
    }
}