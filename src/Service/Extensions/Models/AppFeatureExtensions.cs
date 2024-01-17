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
            entity.AllowAttestation,
            entity.IsGenerateSignInTokenEndpointEnabled,
            entity.IsMagicLinksEnabled);
    }
}