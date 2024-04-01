using AutoFixture;
using FluentAssertions;
using Moq;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Implementations;

public class AuthenticationScopeServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ITenantStorage> _mockTenantStorage;
    private const string Tenant = "app";

    private readonly AuthenticationScopeService _sut;

    public AuthenticationScopeServiceTests()
    {
        _mockTenantStorage = new Mock<ITenantStorage>();
        _mockTenantStorage.SetupGet(x => x.Tenant)
            .Returns(Tenant);

        _sut = new AuthenticationScopeService(_mockTenantStorage.Object);
    }

    [Fact]
    public async Task GetAuthenticationScopesAsync_GivenTenant_WhenNoConfigurationsAreSaved_ThenReturnsTwoDefaultConfigurations()
    {
        // Arrange
        _mockTenantStorage.Setup(x => x.GetAuthenticationConfigurationsAsync())
            .ReturnsAsync(Array.Empty<AuthenticationConfigurationDto>());

        var defaultConfigurations = new List<AuthenticationConfigurationDto>
        {
            AuthenticationConfigurationDto.SignIn(Tenant),
            AuthenticationConfigurationDto.StepUp(Tenant)
        };

        // Act
        var actual = await _sut.GetAuthenticationScopesAsync();

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
        var actual = await _sut.GetAuthenticationScopesAsync();

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
        var actual = await _sut.GetAuthenticationScopesAsync();

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
        var actual = await _sut.GetAuthenticationScopesAsync();

        // Assert
        var actualList = actual.ToList();
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurposes.StepUp.Value);
        actualList.Should().ContainSingle(x => x.Purpose.Value == SignInPurposes.SignIn.Value);
        actualList.Should().Contain(savedConfigurations);
    }
}