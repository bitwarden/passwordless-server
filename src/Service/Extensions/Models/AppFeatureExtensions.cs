using Fido2NetLib.Objects;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Models;

namespace Passwordless.Service.Extensions.Models;

public static class AppFeatureExtensions
{
    public static AppFeatureResponse ToDto(this AppFeature entity)
    {
        if (entity == null) return null;

        return new AppFeatureResponse(
            entity.EventLoggingIsEnabled,
            entity.EventLoggingRetentionPeriod,
            entity.DeveloperLoggingEndsAt,
            entity.MaxUsers,
            entity.Attestation.ToDto(),
            entity.IsGenerateSignInTokenEndpointEnabled);
    }

    public static AttestationConveyancePreference FromDto(this AttestationTypes dto)
    {
        return dto switch
        {
            AttestationTypes.None => AttestationConveyancePreference.None,
            AttestationTypes.Indirect => AttestationConveyancePreference.Indirect,
            AttestationTypes.Direct => AttestationConveyancePreference.Direct,
            AttestationTypes.Enterprise => AttestationConveyancePreference.Enterprise,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, null)
        };
    }

    public static AttestationTypes ToDto(this AttestationConveyancePreference entity)
    {
        return entity switch
        {
            AttestationConveyancePreference.None => AttestationTypes.None,
            AttestationConveyancePreference.Indirect => AttestationTypes.Indirect,
            AttestationConveyancePreference.Direct => AttestationTypes.Direct,
            AttestationConveyancePreference.Enterprise => AttestationTypes.Enterprise,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, null)
        };
    }
}