using AutoFixture;
using FluentAssertions;
using Moq;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Helpers;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class AuthenticationConfigurationServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ITenantStorage> _mockTenantStorage;
    private const string Tenant = "app";

    private readonly AuthenticationConfigurationService _sut;

    public AuthenticationConfigurationServiceTests()
    {
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTenantStorage.SetupGet(x => x.Tenant)
            .Returns(Tenant);

        _sut = new AuthenticationConfigurationService(_mockTenantStorage.Object);
    }

    [Fact]
    public async Task GetAuthenticationScopesAsync_GivenTenant_WhenNoConfigurationsAreSaved_ThenReturnsTwoDefaultConfigurations()
    {
        // Arrange
        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync())
            .ReturnsAsync(Array.Empty<AuthenticationConfigurationDto>());

        var defaultConfigurations = new List<AuthenticationConfigurationDto> { AuthenticationConfigurationDto.SignIn(Tenant), AuthenticationConfigurationDto.StepUp(Tenant) };

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync();

        // Assert
        actual.Should().BeEquivalentTo(defaultConfigurations);
    }

    [Fact]
    public async Task GetAuthenticationScopesAsync_GivenTenant_WhenSignInConfigurationHasChanged_ThenDoesNotReplaceConfigurationWithDefault()
    {
        // Arrange
        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync())
            .ReturnsAsync(_fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, SignInPurposes.SignIn)
                .With(x => x.Tenant, Tenant)
                .CreateMany(1));

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync();

        // Assert
        var actualList = actual.ToList();
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurposes.SignIn.Value);
        var signInConfiguration = actualList.First(x => x.Purpose.Value == SignInPurposes.SignIn.Value);
        signInConfiguration.Should().NotBeSameAs(AuthenticationConfigurationDto.SignIn(Tenant));
    }

    [Fact]
    public async Task GetAuthenticationScopesAsync_GivenTenant_WhenStepUpConfigurationHasChanged_ThenDoesNotReplaceConfigurationWithDefault()
    {
        // Arrange
        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync())
            .ReturnsAsync(_fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, SignInPurposes.SignIn)
                .With(x => x.Tenant, Tenant)
                .CreateMany(1));

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync();

        // Assert
        var actualList = actual.ToList();
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurposes.StepUp.Value);
        var stepUpConfiguration = actualList.First(x => x.Purpose.Value == SignInPurposes.StepUp.Value);
        stepUpConfiguration.Should().NotBeSameAs(AuthenticationConfigurationDto.StepUp(Tenant));
    }

    [Fact]
    public async Task GetAuthenticationScopesAsync_GivenTenant_WhenOtherNonDefaultConfigurationsHaveBeenSaved_ThenConfigurationsAreReturnedWithDefaults()
    {
        // Arrange
        var savedConfigurations = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .CreateMany(3)
            .ToList();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync())
            .ReturnsAsync(savedConfigurations);

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync();

        // Assert
        var actualList = actual.ToList();
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurposes.StepUp.Value);
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurposes.SignIn.Value);
        actualList.Should().Contain(savedConfigurations);
    }

    [Fact]
    public async Task CreateAuthenticationConfigurationAsync_GivenConfiguration_WhenNoMatchingConfigurationExists_ThenConfigurationSaves()
    {
        // Arrange
        var configuration = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationAsync(It.IsAny<SignInPurpose>()))
            .ReturnsAsync((AuthenticationConfigurationDto?)null);

        // Act
        await _sut.CreateAuthenticationConfigurationAsync(configuration);

        // Assert
        _mockTenantStorage.Verify(x => x.CreateAuthenticationConfigurationAsync(configuration), Times.Once);
    }

    [Theory]
    [InlineData(SignInPurposes.SignInName)]
    [InlineData(SignInPurposes.StepUpName)]
    [InlineData("randomPurpose")]
    public async Task CreateAuthenticationConfigurationAsync_GivenConfiguration_WhenMatchesExistingPurpose_ThenThrowsApiExceptionAboutPurposeAlreadyExists(string existingPurpose)
    {
        // Arrange
        var existingSignInPurpose = new SignInPurpose(existingPurpose);

        var configuration = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .With(x => x.Purpose, existingSignInPurpose)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationAsync(existingSignInPurpose))
            .ReturnsAsync(configuration);

        // Act
        var action = () => _sut.CreateAuthenticationConfigurationAsync(configuration);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .WithMessage($"The configuration {existingPurpose} already exists.");
    }

    [Fact]
    public async Task UpdateAuthenticationConfigurationAsync_GivenConfiguration_WhenNoMatchingConfigurationExists_ThenConfigurationSaves()
    {
        // Arrange
        var configuration = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationAsync(It.IsAny<SignInPurpose>()))
            .ReturnsAsync((AuthenticationConfigurationDto?)null);

        // Act
        var action = () => _sut.UpdateAuthenticationConfigurationAsync(configuration);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .WithMessage($"The configuration {configuration.Purpose.Value} does not exist.");
    }

    [Theory]
    [InlineData(SignInPurposes.SignInName)]
    [InlineData(SignInPurposes.StepUpName)]
    [InlineData("randomPurpose")]
    public async Task UpdateAuthenticationConfigurationAsync_GivenConfiguration_WhenMatchesExistingPurpose_ThenThrowsApiExceptionAboutPurposeAlreadyExists(string existingPurpose)
    {
        // Arrange
        var existingSignInPurpose = new SignInPurpose(existingPurpose);

        var configuration = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .With(x => x.Purpose, existingSignInPurpose)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationAsync(existingSignInPurpose))
            .ReturnsAsync(configuration);

        // Act
        await _sut.UpdateAuthenticationConfigurationAsync(configuration);

        // Assert
        _mockTenantStorage.Verify(x => x.UpdateAuthenticationConfigurationAsync(configuration), Times.Once);
    }

    [Fact]
    public async Task DeleteAuthenticationConfigurationAsync_GivenConfiguration_WhenMatchesExistingPurpose_ThenTheConfigurationIsDeleted()
    {
        // Arrange
        var existingSignInPurpose = new SignInPurpose("randomPurpose");

        var configuration = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .With(x => x.Purpose, existingSignInPurpose)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationAsync(existingSignInPurpose))
            .ReturnsAsync(configuration);

        // Act
        await _sut.DeleteAuthenticationConfigurationAsync(configuration);

        // Assert
        _mockTenantStorage.Verify(x => x.DeleteAuthenticationConfigurationAsync(configuration), Times.Once);
    }

    [Theory]
    [InlineData(SignInPurposes.SignInName)]
    [InlineData(SignInPurposes.StepUpName)]
    public async Task DeleteAuthenticationConfigurationAsync_GivenPresetConfiguration_WhenDeleteAttempted_ThenThrowsApiExceptionAboutDeletingPresetConfigurations(string existingPurpose)
    {
        // Arrange
        var existingSignInPurpose = new SignInPurpose(existingPurpose);

        var configuration = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .With(x => x.Purpose, existingSignInPurpose)
            .Create();

        // Act
        var action = () => _sut.DeleteAuthenticationConfigurationAsync(configuration);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .WithMessage($"The {configuration.Purpose.Value} configuration cannot be deleted.");
    }
}