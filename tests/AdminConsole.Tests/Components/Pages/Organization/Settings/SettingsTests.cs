using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Moq;
using Passwordless.AdminConsole.Components.Pages.Organization.SettingsComponents;
using Passwordless.AdminConsole.FeatureManagement;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.Organization.Settings;

public class SettingsTests : BunitContext
{
    private readonly Mock<IDataService> _dataServiceMock = new();
    private readonly Mock<IFeatureManager> _featureManagerMock = new();

    public SettingsTests()
    {
        Services.AddSingleton(_dataServiceMock.Object);
        Services.AddSingleton(_featureManagerMock.Object);

        ComponentFactories.AddStub<DeleteOrganizationComponent>();
        ComponentFactories.AddStub<SecurityComponent>();
    }

    [Fact]
    public void Render_DoesNotRenderSecurityComponent_WhenFeatureIsDisabled()
    {
        // Arrange
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync())
            .ReturnsAsync(new Models.Organization
            {
                Name = "Bitwarden",
                Applications = new List<Application>()
            });
        _featureManagerMock.Setup(x =>
                x.IsEnabledAsync(
                    It.Is<string>(p => p == FeatureFlags.Organization.AllowDisablingMagicLinks)))
            .ReturnsAsync(false);

        // Act
        var cut = Render<AdminConsole.Components.Pages.Organization.Settings>();

        // Assert
        Assert.False(cut.HasComponent<Stub<SecurityComponent>>());
        Assert.True(cut.HasComponent<Stub<DeleteOrganizationComponent>>());
    }

    [Fact]
    public void Render_DoesNotRenderSecurityComponent_WhenFeatureIsEnabled()
    {
        // Arrange
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync())
            .ReturnsAsync(new Models.Organization
            {
                Name = "Bitwarden",
                Applications = new List<Application>()
            });
        _featureManagerMock.Setup(x =>
                x.IsEnabledAsync(
                    It.Is<string>(p => p == FeatureFlags.Organization.AllowDisablingMagicLinks)))
            .ReturnsAsync(true);

        // Act
        var cut = Render<AdminConsole.Components.Pages.Organization.Settings>();

        // Assert
        Assert.True(cut.HasComponent<Stub<SecurityComponent>>());
        Assert.True(cut.HasComponent<Stub<DeleteOrganizationComponent>>());
    }
}