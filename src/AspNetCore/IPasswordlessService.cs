using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Passwordless.AspNetCore;

// TODO: Create options
// Options tell whether to only convert existing or not
// Options also give them the events to tap into

public interface IPasswordlessService<TUser>
    where TUser : class, new()
{
    Task<TUser?> GetUserAsync(HttpContext httpContext);
}

public class PasswordlessService<TUser> : IPasswordlessService<TUser>
    where TUser : class, new()
{
    private readonly IUserStore<TUser> _userStore;

    public PasswordlessService(IUserStore<TUser> userStore)
    {
        _userStore = userStore;
    }

    public async Task<TUser?> GetUserAsync(HttpContext httpContext)
    {
        // Gather services
        if (httpContext.User.Identity?.IsAuthenticated == false && /* TODO: Get from option */ true)
        {
            var user = await CreateUserAsync(httpContext);
            return user;
        }

        var nameIdentifierClaim = httpContext.RequestServices.GetService<IOptions<IdentityOptions>>()?
            .Value.ClaimsIdentity.UserIdClaimType ?? ClaimTypes.NameIdentifier;

        // Create from existing user
        var userId = httpContext.User.FindFirstValue(nameIdentifierClaim);

        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var signedInUser = await _userStore.FindByIdAsync(userId, httpContext.RequestAborted);

        return signedInUser;
    }

    public virtual async Task<TUser?> CreateUserAsync(HttpContext httpContext)
    {
        // Create totally brand new user
        var registerRequest = await httpContext.Request.ReadFromJsonAsync<PasswordlessRegisterRequest>(httpContext.RequestAborted)
            ?? throw new InvalidOperationException("Request body required");

        var user = new TUser();
        await _userStore.SetUserNameAsync(user, registerRequest.Username, httpContext.RequestAborted);
        var result = await _userStore.CreateAsync(user, httpContext.RequestAborted);

        return result.Succeeded ? user : null;
    }
}

record PasswordlessRegisterRequest(string Username, string? DisplayName, HashSet<string>? Aliases);
