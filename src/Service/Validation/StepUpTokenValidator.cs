using Passwordless.Service.Helpers;
using Passwordless.Service.Models;

namespace Passwordless.Service.Validation;

public static class StepUpTokenValidator
{
    public static void Validate(this StepUpToken token, DateTimeOffset now, string context)
    {
        token.Validate(now);

        if (string.Equals(token.Context, context, StringComparison.OrdinalIgnoreCase)) return;

        throw new ApiException("invalid_context", $"The token was not for the given context ({context})", 403);
    }
}