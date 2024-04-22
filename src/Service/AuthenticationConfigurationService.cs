using Microsoft.AspNetCore.Http;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Helpers;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IAuthenticationConfigurationService
{
    Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request);
    Task UpdateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request);
    Task DeleteAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);

    Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync(
        GetAuthenticationConfigurationsFilter filter);

    Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose);
    Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(SignInPurpose purpose);
    Task UpdateLastUsedOnAsync(AuthenticationConfigurationDto config);
}

public class AuthenticationConfigurationService(ITenantStorage storage, TimeProvider timeProvider) : IAuthenticationConfigurationService
{
    public async Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request)
    {
        var existingConfiguration = await GetAuthenticationConfigurationAsync(request.Purpose);

        if (existingConfiguration is not null)
            throw new ApiException($"The configuration {request.Purpose} already exists.",
                StatusCodes.Status400BadRequest);

        await storage.CreateAuthenticationConfigurationAsync(new AuthenticationConfigurationDto
        {
            Purpose = new SignInPurpose(request.Purpose),
            UserVerificationRequirement = request.UserVerificationRequirement,
            TimeToLive = request.TimeToLive,
            Tenant = storage.Tenant,
            CreatedBy = request.PerformedBy,
            CreatedOn = timeProvider.GetUtcNow()
        });
    }

    public async Task UpdateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request)
    {
        var existingConfiguration = await GetAuthenticationConfigurationAsync(request.Purpose);

        if (existingConfiguration is null)
            throw new ApiException($"The configuration {request.Purpose} does not exist.",
                StatusCodes.Status404NotFound);

        if (IsUnsavedPresetConfiguration(existingConfiguration))
        {
            await storage.CreateAuthenticationConfigurationAsync(existingConfiguration);
        }

        existingConfiguration.UserVerificationRequirement = request.UserVerificationRequirement;
        existingConfiguration.TimeToLive = request.TimeToLive;
        existingConfiguration.EditedBy = request.PerformedBy;
        existingConfiguration.EditedOn = timeProvider.GetUtcNow();

        await storage.UpdateAuthenticationConfigurationAsync(existingConfiguration);
    }

    public Task DeleteAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration)
    {
        if (IsNotSignInConfiguration(configuration) && IsNotStepUpConfiguration(configuration))
            return storage.DeleteAuthenticationConfigurationAsync(configuration);

        throw new ApiException($"The {configuration.Purpose.Value} configuration cannot be deleted.", StatusCodes.Status400BadRequest);
    }

    public async Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync(GetAuthenticationConfigurationsFilter filter)
    {
        var configurations = (await storage.GetAuthenticationConfigurationsAsync(filter)).ToList();

        if (!string.IsNullOrWhiteSpace(filter.Purpose) && configurations.Count == 0)
        {
            var presetConfig = GetPresetDefaults(filter.Purpose);

            if (presetConfig is not null) configurations.Add(presetConfig);

            return configurations;
        }

        if (configurations.All(IsNotStepUpConfiguration)) configurations.Add(AuthenticationConfigurationDto.StepUp(storage.Tenant));

        if (configurations.All(IsNotSignInConfiguration)) configurations.Add(AuthenticationConfigurationDto.SignIn(storage.Tenant));

        return configurations;
    }

    public async Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(string purpose)
    {
        var configuration = (await storage.GetAuthenticationConfigurationsAsync(
            new GetAuthenticationConfigurationsFilter
            {
                Purpose = purpose
            })).FirstOrDefault() ?? GetPresetDefaults(purpose);

        return configuration;
    }

    public async Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(
        SignInPurpose purpose)
    {
        var configuration = (await storage.GetAuthenticationConfigurationsAsync(
                new GetAuthenticationConfigurationsFilter { Purpose = purpose.Value })).FirstOrDefault();

        return configuration ?? GetDefault(purpose);
    }

    public async Task UpdateLastUsedOnAsync(AuthenticationConfigurationDto config)
    {
        var existingConfiguration = await GetAuthenticationConfigurationAsync(config.Purpose.Value);

        if (existingConfiguration is null)
            throw new ApiException($"The configuration {config.Purpose.Value} does not exist.",
                StatusCodes.Status404NotFound);

        if (IsUnsavedPresetConfiguration(existingConfiguration))
            await storage.CreateAuthenticationConfigurationAsync(existingConfiguration);

        existingConfiguration.LastUsedOn = timeProvider.GetUtcNow();

        await storage.UpdateAuthenticationConfigurationAsync(existingConfiguration);
    }

    private AuthenticationConfigurationDto GetDefault(SignInPurpose purpose) => purpose.Value switch
    {
        SignInPurpose.StepUpName or SignInPurpose.SignInName => GetPresetDefaults(purpose.Value)!,
        _ => AuthenticationConfigurationDto.SignIn(storage.Tenant)
    };

    private AuthenticationConfigurationDto? GetPresetDefaults(string purpose) => purpose switch
    {
        SignInPurpose.StepUpName => AuthenticationConfigurationDto.StepUp(storage.Tenant),
        SignInPurpose.SignInName => AuthenticationConfigurationDto.SignIn(storage.Tenant),
        _ => null
    };

    private static bool IsNotStepUpConfiguration(AuthenticationConfigurationDto configuration) =>
        !IsStepUpConfiguration(configuration);

    private static bool IsNotSignInConfiguration(AuthenticationConfigurationDto configuration) =>
        !IsSignInConfiguration(configuration);

    private static bool IsUnsavedPresetConfiguration(AuthenticationConfigurationDto request) =>
        request.EditedOn is null && IsSignInConfiguration(request) || IsStepUpConfiguration(request);

    private static bool IsSignInConfiguration(AuthenticationConfigurationDto request) =>
        request.Purpose == SignInPurpose.SignIn;

    private static bool IsStepUpConfiguration(AuthenticationConfigurationDto request) =>
        request.Purpose == SignInPurpose.StepUp;
}