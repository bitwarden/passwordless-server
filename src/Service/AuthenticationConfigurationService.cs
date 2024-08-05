using Microsoft.AspNetCore.Http;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.EventLog.Loggers;
using Passwordless.Service.Helpers;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service;

public interface IAuthenticationConfigurationService
{
    Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request);
    Task UpdateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request);
    Task DeleteAuthenticationConfigurationAsync(AuthenticationConfigurationDto configuration);

    Task<IEnumerable<AuthenticationConfigurationDto>> GetAuthenticationConfigurationsAsync(GetAuthenticationConfigurationsFilter filter);

    Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(SignInPurpose purpose);
    Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(SignInPurpose purpose);
    Task UpdateLastUsedOnAsync(AuthenticationConfigurationDto config);
}

public class AuthenticationConfigurationService(ITenantStorage storage, TimeProvider timeProvider, IEventLogger eventLogger) : IAuthenticationConfigurationService
{
    public async Task CreateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request)
    {
        var configuration = new AuthenticationConfigurationDto
        {
            Purpose = new SignInPurpose(request.Purpose),
            UserVerificationRequirement = request.UserVerificationRequirement,
            TimeToLive = request.TimeToLive,
            Hints = request.Hints,
            Tenant = storage.Tenant,
            CreatedBy = request.PerformedBy,
            CreatedOn = timeProvider.GetUtcNow()
        };

        var existingConfiguration = await GetAuthenticationConfigurationAsync(configuration.Purpose);

        if (existingConfiguration is not null)
            throw new ApiException($"The configuration {configuration.Purpose.Value} already exists.",
                StatusCodes.Status400BadRequest);

        await storage.CreateAuthenticationConfigurationAsync(configuration);

        eventLogger.LogAuthenticationConfigurationCreated(configuration);
    }

    public async Task UpdateAuthenticationConfigurationAsync(SetAuthenticationConfigurationRequest request)
    {
        var existingConfiguration = await GetAuthenticationConfigurationAsync(new SignInPurpose(request.Purpose));

        if (existingConfiguration is null)
            throw new ApiException($"The configuration {request.Purpose} does not exist.",
                StatusCodes.Status404NotFound);

        if (IsUnsavedPresetConfiguration(existingConfiguration))
        {
            await storage.CreateAuthenticationConfigurationAsync(existingConfiguration);
        }

        existingConfiguration.UserVerificationRequirement = request.UserVerificationRequirement;
        existingConfiguration.TimeToLive = request.TimeToLive;
        existingConfiguration.Hints = request.Hints;
        existingConfiguration.EditedBy = request.PerformedBy;
        existingConfiguration.EditedOn = timeProvider.GetUtcNow();

        await storage.UpdateAuthenticationConfigurationAsync(existingConfiguration);

        eventLogger.LogAuthenticationConfigurationUpdated(existingConfiguration);
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

    public async Task<AuthenticationConfigurationDto?> GetAuthenticationConfigurationAsync(SignInPurpose purpose)
    {
        var configuration = (await storage.GetAuthenticationConfigurationsAsync(
            new GetAuthenticationConfigurationsFilter
            {
                Purpose = purpose.Value
            })).FirstOrDefault() ?? GetPresetDefaults(purpose.Value);

        return configuration;
    }

    public async Task<AuthenticationConfigurationDto> GetAuthenticationConfigurationOrDefaultAsync(SignInPurpose purpose)
    {
        var configuration = (await storage.GetAuthenticationConfigurationsAsync(
                new GetAuthenticationConfigurationsFilter { Purpose = purpose.Value })).FirstOrDefault();

        return configuration ?? GetDefault(purpose);
    }

    public async Task UpdateLastUsedOnAsync(AuthenticationConfigurationDto config)
    {
        var existingConfiguration = await GetAuthenticationConfigurationAsync(config.Purpose);

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
        request.EditedOn is null && request.LastUsedOn is null
                                 && (IsSignInConfiguration(request) || IsStepUpConfiguration(request));

    private static bool IsSignInConfiguration(AuthenticationConfigurationDto request) =>
        request.Purpose == SignInPurpose.SignIn;

    private static bool IsStepUpConfiguration(AuthenticationConfigurationDto request) =>
        request.Purpose == SignInPurpose.StepUp;
}