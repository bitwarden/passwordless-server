using Passwordless.Common.Models.Apps;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public class ApplicationService : IApplicationService
{
    private readonly ITenantStorage _storage;
    private readonly IEventLogger _eventLogger;

    public ApplicationService(ITenantStorage storage, IEventLogger eventLogger)
    {
        _storage = storage;
        _eventLogger = eventLogger;
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
}