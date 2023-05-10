using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Passwordless.Net;

namespace Passwordless.AspNetCore;

public interface IPasswordlessIdentityService
{
    Task<VerifiedUser?> SignInUserAsync(string token);
    Task<string> ConvertUserAsync();
}

internal class PasswordlessIdentityService<TUser> : IPasswordlessIdentityService
    where TUser : class
{
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly SignInManager<TUser> _signInManager;
    private readonly PasswordlessAspNetCoreOptions _passwordlessOptions;

    public PasswordlessIdentityService(
        IPasswordlessClient passwordlessApi,
        SignInManager<TUser> signInManager,
        IOptions<PasswordlessAspNetCoreOptions> optionsAccessor
    )
    {
        _passwordlessClient = passwordlessApi;
        _signInManager = signInManager;
        _passwordlessOptions = optionsAccessor.Value;
    }

    public async Task<string> ConvertUserAsync()
    {
        var signedInUser = await _signInManager.UserManager.GetUserAsync(_signInManager.Context.User);

        if (signedInUser == null)
        {
            throw new Exception("Bad");
        }

        var userId = await _signInManager.UserManager.GetUserIdAsync(signedInUser);
        var userName = await _signInManager.UserManager.GetUserNameAsync(signedInUser);

        var httpContext = _signInManager.Context;

        var context = new ConvertUserContext
        {
            HttpContext = httpContext,
            RegisterOptions = new RegisterOptions
            {
                UserId = userId,
                Username = userName ?? "",
                DisplayName = userName ?? httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                AliasHashing = false,
                Discoverable = true
            }
        };

        if (userName != null)
        {
            context.RegisterOptions.Aliases = new HashSet<string>() { userName };
        }

        await _passwordlessOptions.ConvertUser(context);

        var tokenResponse = await _passwordlessClient.CreateRegisterToken(context.RegisterOptions);
        return tokenResponse.Token;
    }

    public async Task<VerifiedUser?> SignInUserAsync(string token)
    {
        VerifiedUser? verifiedUser = await _passwordlessClient.VerifyToken(token);

        if (verifiedUser == null)
        {
            throw new Exception("Bad");
        }

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(_signInManager.Options.ClaimsIdentity.UserIdClaimType, verifiedUser.UserId),
        }));

        var user = await _signInManager.UserManager.GetUserAsync(claimsPrincipal);

        if (user == null)
        {
            await _signInManager.Context.ChallengeAsync();
            return null;
        }

        // TODO: This is probably where we create an optional extensibility point
        await _signInManager.SignInAsync(user, true);
        return verifiedUser;
    }
}