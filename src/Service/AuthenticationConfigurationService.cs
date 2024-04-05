using Microsoft.AspNetCore.Http;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IAuthenticationConfigurationService
{
    Task CreateAuthenticationConfigurationAsync();
    Task SetAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);
    Task DeleteAuthenticationConfigurationAsync();
    Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync();
    Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose);
    Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(SignInPurpose purpose);
}

public class AuthenticationConfigurationService(ITenantStorage storage) : IAuthenticationConfigurationService
{
    public Task CreateAuthenticationConfigurationAsync()
    {
        throw new NotImplementedException();
    }

    public async Task SetAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration)
    {
        var configurations = await GetAuthenticationConfigurationsAsync();

        if (configurations.Select(x => x.Purpose.Value).Contains(configuration.Purpose.Value, StringComparer.OrdinalIgnoreCase))
            throw new ApiException($"The configuration {configuration.Purpose.Value} already exists.", StatusCodes.Status400BadRequest);

        await storage.CreateAuthenticationConfigurationAsync(configuration);
    }

    public Task DeleteAuthenticationConfigurationAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync()
    {
        var configurations = await storage.GetAuthenticationConfigurationsAsync();

        var result = configurations.ToList();

        if (result.All(IsNotStepUpConfiguration)) result.Add(AuthenticationConfigurationDto.StepUp(storage.Tenant));

        if (result.All(IsNotSignInConfiguration)) result.Add(AuthenticationConfigurationDto.SignIn(storage.Tenant));

        return result;
    }

    public Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose) =>
        storage.GetAuthenticationConfigurationAsync(new SignInPurpose(purpose));

    public async Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(SignInPurpose purpose)
    {
        var configuration = await storage.GetAuthenticationConfigurationAsync(purpose);

        return configuration ?? GetDefault(purpose);
    }

    private AuthenticationConfigurationDto GetDefault(SignInPurpose purpose) =>
        purpose == SignInPurposes.StepUp
            ? AuthenticationConfigurationDto.StepUp(storage.Tenant)
            : AuthenticationConfigurationDto.SignIn(storage.Tenant);

    private static bool IsNotStepUpConfiguration(AuthenticationConfigurationDto configuration) => configuration.Purpose != SignInPurposes.StepUp;
    private static bool IsNotSignInConfiguration(AuthenticationConfigurationDto configuration) => configuration.Purpose != SignInPurposes.SignIn;
}