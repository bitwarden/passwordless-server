using Microsoft.AspNetCore.Http;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IAuthenticationConfigurationService
{
    Task CreateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);
    Task UpdateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);
    Task DeleteAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);
    Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync();
    Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose);
    Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(SignInPurpose purpose);
}

public class AuthenticationConfigurationService(ITenantStorage storage) : IAuthenticationConfigurationService
{
    public async Task CreateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration)
    {
        var existingConfiguration = await GetAuthenticationConfigurationAsync(configuration.Purpose.Value);

        if (existingConfiguration is not null) throw new ApiException($"The configuration {configuration.Purpose.Value} already exists.", StatusCodes.Status400BadRequest);

        await storage.CreateAuthenticationConfigurationAsync(configuration);
    }

    public async Task UpdateAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration)
    {
        var existingConfiguration = await GetAuthenticationConfigurationAsync(configuration.Purpose.Value);

        if (existingConfiguration is null) throw new ApiException($"The configuration {configuration.Purpose.Value} does not exist.", StatusCodes.Status404NotFound);

        await storage.UpdateAuthenticationConfigurationAsync(configuration);
    }

    public Task DeleteAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration) =>
        storage.DeleteAuthenticationConfigurationAsync(configuration);

    public async Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync()
    {
        var configurations = await storage.GetAuthenticationConfigurationsAsync();

        var result = configurations.ToList();

        if (result.All(IsNotStepUpConfiguration)) result.Add(AuthenticationConfigurationDto.StepUp(storage.Tenant));

        if (result.All(IsNotSignInConfiguration)) result.Add(AuthenticationConfigurationDto.SignIn(storage.Tenant));

        return result;
    }

    public async Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose)
    {
        var configuration = await storage.GetAuthenticationConfigurationAsync(new SignInPurpose(purpose)) ?? GetPresetDefaults(purpose);

        return configuration;
    }

    public async Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(SignInPurpose purpose)
    {
        var configuration = await storage.GetAuthenticationConfigurationAsync(purpose);

        return configuration ?? GetDefault(purpose);
    }

    private AuthenticationConfigurationDto GetDefault(SignInPurpose purpose) => purpose.Value switch
    {
        SignInPurposes.StepUpName or SignInPurposes.SignInName => GetPresetDefaults(purpose.Value)!,
        _ => AuthenticationConfigurationDto.SignIn(storage.Tenant)
    };

    private AuthenticationConfigurationDto? GetPresetDefaults(string purpose) => purpose switch
    {
        SignInPurposes.StepUpName => AuthenticationConfigurationDto.StepUp(storage.Tenant),
        SignInPurposes.SignInName => AuthenticationConfigurationDto.SignIn(storage.Tenant),
        _ => null
    };

    private static bool IsNotStepUpConfiguration(AuthenticationConfigurationDto configuration) => configuration.Purpose != SignInPurposes.StepUp;
    private static bool IsNotSignInConfiguration(AuthenticationConfigurationDto configuration) => configuration.Purpose != SignInPurposes.SignIn;
}