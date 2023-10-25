using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Passwordless.AdminConsole.EventLog.Loggers;

namespace Passwordless.AdminConsole.Helpers;

public class LoginEndpointFilter : IEndpointFilter
{
    private readonly IEventLogger _logger;
    private readonly ISystemClock _systemClock;

    public LoginEndpointFilter(IEventLogger logger, ISystemClock systemClock)
    {
        _logger = logger;
        _systemClock = systemClock;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is SignInHttpResult { Principal.Identity.IsAuthenticated: true } signInHttpResult)
        {
            _logger.LogAdminLoginTokenVerifiedEvent(signInHttpResult.Principal, _systemClock.UtcNow.UtcDateTime);
        }

        return result;
    }
}