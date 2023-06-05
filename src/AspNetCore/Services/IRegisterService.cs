using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Passwordless.Net;

namespace Passwordless.AspNetCore.Services;

public interface IRegisterService<TBody>
{
    Task<Results<Ok<RegisterTokenResponse>, ValidationProblem>> RegisterAsync(TBody body, CancellationToken cancellationToken);
}

public class RegisterService<TUser> : IRegisterService<PasswordlessRegisterRequest>
    where TUser : class, new()
{
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IUserStore<TUser> _userStore;
    private readonly ILogger<RegisterService<TUser>> _logger;

    public RegisterService(IPasswordlessClient passwordlessClient, IUserStore<TUser> userStore, ILogger<RegisterService<TUser>> logger)
    {
        _passwordlessClient = passwordlessClient;
        _userStore = userStore;
        _logger = logger;
    }

    public virtual async Task<Results<Ok<RegisterTokenResponse>, ValidationProblem>> RegisterAsync(PasswordlessRegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new TUser();
        await BindToUserAsync(user, request, cancellationToken);
        var result = await _userStore.CreateAsync(user, cancellationToken);

        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
        }

        var userId = await _userStore.GetUserIdAsync(user, cancellationToken);
        _logger.LogInformation("Created user with username {Username} and id {Id}", request.Username, userId);

        // Call passwordless
        var registerTokenResponse = await _passwordlessClient.CreateRegisterToken(new RegisterOptions
        {
            UserId = userId,
            DisplayName = request.DisplayName,
            Username = request.Username,
            Aliases = request.Aliases,
        });

        return TypedResults.Ok(registerTokenResponse);
    }

    public virtual async Task BindToUserAsync(TUser user, PasswordlessRegisterRequest request, CancellationToken cancellationToken)
    {
        await _userStore.SetUserNameAsync(user, request.Username, cancellationToken);
    }
}

public record PasswordlessRegisterRequest(string Username, string? DisplayName, HashSet<string>? Aliases);
