using Humanizer;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;

namespace Passwordless.Service.Extensions.Models;

public static class TokenExtensions
{
    public static void Validate(this Token token, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().DateTime;

        if (token.ExpiresAt >= now)
        {
            return;
        }

        var drift = now - token.ExpiresAt;

        throw new ApiException("expired_token", $"The token expired {drift.Humanize()} ago.", 403);
    }
}