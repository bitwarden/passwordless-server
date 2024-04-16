using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Passwordless.Common.Models.Apps;
using Passwordless.Service.Helpers;
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

        _sut = new AuthenticationConfigurationService(_mockTenantStorage.Object, new FakeTimeProvider());
    }

    [Fact]
    public async Task GetAuthenticationConfigurationsAsync_GivenTenant_WhenNoConfigurationsAreSaved_ThenReturnsTwoDefaultConfigurations()
    {
        // Arrange
        _mockTenantStorage.Setup(x =>
                x.GetAuthenticationConfigurationsAsync(It.IsAny<GetAuthenticationConfigurationsFilter>()))
            .ReturnsAsync(Array.Empty<AuthenticationConfigurationDto>());

        var defaultConfigurations = new List<AuthenticationConfigurationDto>
        {
            AuthenticationConfigurationDto.SignIn(Tenant), AuthenticationConfigurationDto.StepUp(Tenant)
        };

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync(new GetAuthenticationConfigurationsFilter());

        // Assert
        actual.Should().BeEquivalentTo(defaultConfigurations);
    }

    [Fact]
    public async Task GetAuthenticationConfigurationsAsync_GivenTenant_WhenSignInConfigurationHasChanged_ThenDoesNotReplaceConfigurationWithDefault()
    {
        // Arrange
        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(It.IsAny<GetAuthenticationConfigurationsFilter>()))
            .ReturnsAsync(_fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, SignInPurpose.SignIn)
                .With(x => x.Tenant, Tenant)
                .CreateMany(1));

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync(new GetAuthenticationConfigurationsFilter());

        // Assert
        var actualList = actual.ToList();
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurpose.SignIn.Value);
        var signInConfiguration = actualList.First(x => x.Purpose.Value == SignInPurpose.SignIn.Value);
        signInConfiguration.Should().NotBeSameAs(AuthenticationConfigurationDto.SignIn(Tenant));
    }

    [Fact]
    public async Task GetAuthenticationConfigurationsAsync_GivenTenant_WhenStepUpConfigurationHasChanged_ThenDoesNotReplaceConfigurationWithDefault()
    {
        // Arrange
        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(It.IsAny<GetAuthenticationConfigurationsFilter>()))
            .ReturnsAsync(_fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, SignInPurpose.StepUp)
                .With(x => x.Tenant, Tenant)
                .CreateMany(1));

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync(new GetAuthenticationConfigurationsFilter());

        // Assert
        var actualList = actual.ToList();
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurpose.StepUp.Value);
        var stepUpConfiguration = actualList.First(x => x.Purpose.Value == SignInPurpose.StepUp.Value);
        stepUpConfiguration.Should().NotBeSameAs(AuthenticationConfigurationDto.StepUp(Tenant));
    }

    [Fact]
    public async Task GetAuthenticationConfigurationsAsync_GivenTenant_WhenOtherNonDefaultConfigurationsHaveBeenSaved_ThenConfigurationsAreReturnedWithDefaults()
    {
        // Arrange
        var savedConfigurations = _fixture.Build<AuthenticationConfigurationDto>()
            .With(x => x.Tenant, Tenant)
            .CreateMany(3)
            .ToList();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(
                It.IsAny<GetAuthenticationConfigurationsFilter>())
            ).ReturnsAsync(savedConfigurations);

        // Act
        var actual = await _sut.GetAuthenticationConfigurationsAsync(new GetAuthenticationConfigurationsFilter());

        // Assert
        var actualList = actual.ToList();
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurpose.StepUp.Value);
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurpose.SignIn.Value);
        actualList.Should().Contain(savedConfigurations);
    }

    [Fact]
    public async Task CreateAuthenticationConfigurationAsync_GivenConfiguration_WhenNoMatchingConfigurationExists_ThenConfigurationSaves()
    {
        // Arrange
        var configuration = _fixture.Create<SetAuthenticationConfigurationRequest>();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(
            It.Is<GetAuthenticationConfigurationsFilter>(y =>
                y.Purpose == configuration.Purpose))
        ).ReturnsAsync(new List<AuthenticationConfigurationDto>());

        // Act
        await _sut.CreateAuthenticationConfigurationAsync(configuration);

        // Assert
        _mockTenantStorage.Verify(
            x => x.CreateAuthenticationConfigurationAsync(
                It.Is<AuthenticationConfigurationDto>(dto => dto.Purpose.Value == configuration.Purpose)
            ), Times.Once);
    }

    [Theory]
    [InlineData(SignInPurpose.SignInName)]
    [InlineData(SignInPurpose.StepUpName)]
    [InlineData("randomPurpose")]
    public async Task CreateAuthenticationConfigurationAsync_GivenConfiguration_WhenMatchesExistingPurpose_ThenThrowsApiExceptionAboutPurposeAlreadyExists(string existingPurpose)
    {
        // Arrange
        var configuration = _fixture.Build<SetAuthenticationConfigurationRequest>()
            .With(x => x.Purpose, existingPurpose)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(
            It.Is<GetAuthenticationConfigurationsFilter>(y => y.Purpose == configuration.Purpose)
        )).ReturnsAsync(new[]
        {
            _fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, new SignInPurpose(existingPurpose))
                .Create()
        });

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
        var configuration = _fixture.Create<SetAuthenticationConfigurationRequest>();

        _mockTenantStorage.Setup(x =>
            x.GetAuthenticationConfigurationsAsync(
                It.Is<GetAuthenticationConfigurationsFilter>(y => y.Purpose == configuration.Purpose))
        ).ReturnsAsync(new List<AuthenticationConfigurationDto>());

        // Act
        var action = () => _sut.UpdateAuthenticationConfigurationAsync(configuration);

        // Assert
        await action.Should().ThrowAsync<ApiException>()
            .WithMessage($"The configuration {configuration.Purpose} does not exist.");
    }

    [Fact]
    public async Task UpdateAuthenticationConfigurationAsync_GivenConfiguration_WhenMatchesExistingPurpose_ThenTheConfigurationIsUpdated()
    {
        // Arrange
        var configuration = _fixture.Create<SetAuthenticationConfigurationRequest>();

        _mockTenantStorage.Setup(x =>
            x.GetAuthenticationConfigurationsAsync(
                It.Is<GetAuthenticationConfigurationsFilter>(y => y.Purpose == configuration.Purpose)
            )
        ).ReturnsAsync(new[]
        {
            _fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, new SignInPurpose(configuration.Purpose))
                .Create()
        });

        // Act
        await _sut.UpdateAuthenticationConfigurationAsync(configuration);

        // Assert
        _mockTenantStorage.Verify(x => x.UpdateAuthenticationConfigurationAsync(
            It.Is<AuthenticationConfigurationDto>(dto => dto.Purpose.Value == configuration.Purpose)
        ), Times.Once);
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

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(
            It.Is<GetAuthenticationConfigurationsFilter>(y => y.Purpose == configuration.Purpose.Value))
        ).ReturnsAsync(new[] { configuration });

        // Act
        await _sut.DeleteAuthenticationConfigurationAsync(configuration);

        // Assert
        _mockTenantStorage.Verify(x => x.DeleteAuthenticationConfigurationAsync(configuration), Times.Once);
    }

    [Theory]
    [InlineData(SignInPurpose.SignInName)]
    [InlineData(SignInPurpose.StepUpName)]
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

    [Theory]
    [InlineData(SignInPurpose.SignInName)]
    [InlineData(SignInPurpose.StepUpName)]
    public async Task UpdateAuthenticationConfigurationAsync_GivenEditedConfiguration_WhenEditedConfigurationIsAPresetAndNonExistent_ThenTheConfigurationIsCreated(string presetPurpose)
    {
        // Arrange
        var request = _fixture.Build<SetAuthenticationConfigurationRequest>()
            .With(x => x.Purpose, presetPurpose)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(
            It.Is<GetAuthenticationConfigurationsFilter>(y => y.Purpose == request.Purpose))
        ).ReturnsAsync(new[] {
            _fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, new SignInPurpose(request.Purpose))
                .Create()
        });

        // Action
        await _sut.UpdateAuthenticationConfigurationAsync(request);

        // Assert
        _mockTenantStorage.Verify(x => x.CreateAuthenticationConfigurationAsync(
            It.Is<AuthenticationConfigurationDto>(dto => dto.Purpose.Value == request.Purpose)
        ), Times.Once);
    }

    [Theory]
    [InlineData(SignInPurpose.SignInName)]
    [InlineData(SignInPurpose.StepUpName)]
    public async Task UpdateAuthenticationConfigurationAsync_GivenEditedConfiguration_WhenEditedConfigurationIsAPresetAndAlreadyExists_ThenTheConfigurationIsCreated(string presetPurpose)
    {
        // Arrange
        var request = _fixture.Build<SetAuthenticationConfigurationRequest>()
            .With(x => x.Purpose, presetPurpose)
            .Create();

        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync(
            It.Is<GetAuthenticationConfigurationsFilter>(y => y.Purpose == request.Purpose))
        ).ReturnsAsync(new[] {
            _fixture.Build<AuthenticationConfigurationDto>()
                .With(x => x.Purpose, new SignInPurpose(request.Purpose))
                .Create()
        });

        // Action
        await _sut.UpdateAuthenticationConfigurationAsync(request);

        // Assert
        _mockTenantStorage.Verify(x => x.UpdateAuthenticationConfigurationAsync(
            It.Is<AuthenticationConfigurationDto>(dto => dto.Purpose.Value == request.Purpose)
        ), Times.Once);
    }
}