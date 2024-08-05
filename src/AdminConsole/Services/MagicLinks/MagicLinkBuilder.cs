using System.Text;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Passwordless.AdminConsole.Identity;
using Passwordless.Common.Extensions;
using Passwordless.Models;

namespace Passwordless.AdminConsole.Services.MagicLinks;

public class MagicLinkBuilder(
    SignInManager<ConsoleAdmin> signInManager,
    IPasswordlessClient passwordlessClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<MagicLinkBuilder> logger)
    : IMagicLinkBuilder
{
    public async Task<string> GetLinkAsync(string email, string? returnUrl = null)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogError("User not found: {email}", email);
            throw new ArgumentException("User not found", nameof(email));
        }

        var token = await passwordlessClient.GenerateAuthenticationTokenAsync(new AuthenticationOptions(user.Id));

        var baseUrl = httpContextAccessor.HttpContext!.Request.GetBaseUrl();
        var encodedToken = HttpUtility.UrlEncode(token.Token);
        var encodedReturnUrl = HttpUtility.UrlEncode(returnUrl);
        var urlBuilder = new StringBuilder($"{baseUrl}/Account/Magic?token={encodedToken}&returnUrl={encodedReturnUrl}");

        return urlBuilder.ToString();
    }

    public string GetUrlTemplate(string? returnUrl = null)
    {
        var baseUrl = httpContextAccessor.HttpContext!.Request.GetBaseUrl();

        var urlBuilder = new StringBuilder($"{baseUrl}/Account/Magic?token=$TOKEN");

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            var encodedReturnUrl = HttpUtility.UrlEncode(returnUrl);
            urlBuilder.Append($"&returnUrl={encodedReturnUrl}");
        }

        return urlBuilder.ToString();
    }
}