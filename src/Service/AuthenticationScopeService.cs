using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IAuthenticationScopeService
{
    Task CreateAuthenticationScopeAsync();
    Task SetAuthenticationScopeAsync();
    Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationScopesAsync();
}

public class AuthenticationScopeService(ITenantStorage storage) : IAuthenticationScopeService
{
    public Task CreateAuthenticationScopeAsync()
    {
        throw new NotImplementedException();
    }

    public Task SetAuthenticationScopeAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationScopesAsync()
    {
        var configurations = await storage.GetAuthenticationConfigurationsAsync();

        var result = configurations.ToList();

        if (result.All(IsNotStepUpConfiguration)) result.Add(AuthenticationConfigurationDto.StepUp(storage.Tenant));

        if (result.All(IsNotSignInConfiguration)) result.Add(AuthenticationConfigurationDto.SignIn(storage.Tenant));

        return result;
    }

    private static bool IsNotStepUpConfiguration(AuthenticationConfigurationDto configuration) => configuration.Purpose != SignInPurposes.StepUp;
    private static bool IsNotSignInConfiguration(AuthenticationConfigurationDto configuration) => configuration.Purpose != SignInPurposes.SignIn;
}