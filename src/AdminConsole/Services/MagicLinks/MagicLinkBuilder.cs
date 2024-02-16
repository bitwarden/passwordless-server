using System.Collections.Specialized;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Passwordless.AdminConsole.Identity;
using Passwordless.Models;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkBuilder : IMagicLinkBuilder
{
    private readonly SignInManager<ConsoleAdmin> _signInManager;
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly ILogger<MagicLinkBuilder> _logger;

    public MagicLinkBuilder(
        SignInManager<ConsoleAdmin> signInManager,
        IPasswordlessClient passwordlessClient,
        IActionContextAccessor actionContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        ILogger<MagicLinkBuilder> logger)
    {
        _signInManager = signInManager;
        _passwordlessClient = passwordlessClient;
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

        var token = await _passwordlessClient.GenerateAuthenticationTokenAsync(new AuthenticationOptions(user.Id));
        var urlBuilder = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var url = urlBuilder.PageLink("/Account/Magic", values: new { returnUrl, token }) ?? urlBuilder.Content("~/");

       return url;
    }

    public string GetUrlTemplate(string? returnUrl = null)
    {
        var urlBuilder = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        
        var url = urlBuilder.PageLink("/Account/Magic", values: new { token = "__token__" }) ?? urlBuilder.Content("~/");
        
        return url;
    }
}