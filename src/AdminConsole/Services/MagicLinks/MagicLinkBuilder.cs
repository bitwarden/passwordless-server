using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Passwordless.AdminConsole.Identity;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkBuilder : IMagicLinkBuilder
{
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly ILogger<MagicLinkBuilder> _logger;

    public MagicLinkBuilder(
        SignInManager<ConsoleAdmin> signInManager,
        IActionContextAccessor actionContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        ILogger<MagicLinkBuilder> logger)
    {
        _signInManager = signInManager;
        _actionContextAccessor = actionContextAccessor;
        _urlHelperFactory = urlHelperFactory;
        _logger = logger;
    }

    public async Task<string> GetLinkAsync(string email, string? returnUrl = null)
    {
        var user = await _signInManager.UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogError("User not found: {email}", email);
            throw new ArgumentException("User not found", nameof(email));
        }

        if (_actionContextAccessor.ActionContext == null)
        {
            _logger.LogError("ActionContext is null");
            throw new InvalidOperationException("ActionContext is null");
        }

        var token = await _signInManager.UserManager.GenerateUserTokenAsync(user, _signInManager.Options.Tokens.PasswordResetTokenProvider, SignInPurposes.MagicLink);
        var urlBuilder = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var url = urlBuilder.PageLink("/Account/Magic", values: new { returnUrl }) ?? urlBuilder.Content("~/");

        UriBuilder uriBuilder = new(url);
        NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["token"] = token;
        query["email"] = email;
        uriBuilder.Query = query.ToString();

        return uriBuilder.ToString();
    }
}