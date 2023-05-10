using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

#nullable enable

namespace Passwordless.Api.Authorization;

public class HeaderOptions<TDep> : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = null!;
    public Func<TDep, string, Task<Claim[]>> ClaimsCreator { get; set; } = null!;
    public IProblemDetailWriter? ProblemDetailWriter { get; set; }
}

public interface IProblemDetailWriter
{
    IEnumerable<string> GetDetails(HttpContext context, string headerName);
}

public class HeaderHandler<TDep> : AuthenticationHandler<HeaderOptions<TDep>>
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly TDep _dependency;

    public HeaderHandler(
        IOptionsMonitor<HeaderOptions<TDep>> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IProblemDetailsService problemDetailsService,
        TDep dependency)
        : base(options, logger, encoder, clock)
    {
        _problemDetailsService = problemDetailsService;
        _dependency = dependency;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var headerValues))
        {
            return AuthenticateResult.NoResult();
        }

        var headerValue = headerValues.First()!;

        try
        {
            var claims = await Options.ClaimsCreator.Invoke(_dependency, headerValue);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return AuthenticateResult.Success(new AuthenticationTicket(principal, null, Scheme.Name));
        }
        catch (Exception exception)
        {
            return AuthenticateResult.Fail(exception);
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.Append(HeaderNames.WWWAuthenticate, Scheme.Name);
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = Context,
            ProblemDetails = new ProblemDetails
            {
                Title = $"A valid '{Options.HeaderName}' header is required.",
                Status = StatusCodes.Status401Unauthorized,
                Type = $"https://docs.passwordless.dev/guide/errors.html#{Scheme.Name}",
                Detail = GatherDetail(),
            },
        });
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = Context,
            ProblemDetails = new ProblemDetails
            {
                Title = "You are forbidden from viewing this resource.",
                Status = StatusCodes.Status403Forbidden,
                Type = "https://docs.passwordless.dev/guide/errors.html#forbidden",
            },
        });
    }

    private string? GatherDetail()
    {
        var details = Options.ProblemDetailWriter?.GetDetails(Context, Options.HeaderName).ToList();
        if (details == null || details.Count == 0)
        {
            return null;
        }

        return string.Join(", ", details);
    }
}