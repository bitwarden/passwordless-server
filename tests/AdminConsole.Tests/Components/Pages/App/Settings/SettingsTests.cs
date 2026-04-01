using AutoFixture;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Settings;

public class SettingsTests : BunitContext
{
    private readonly Fixture _fixture = new();

    private readonly Mock<ICurrentContext> _currentContextMock = new();
    private readonly Mock<IDataService> _dataServiceMock = new();

    private const string PlanSectionId = "plan-section";
    private const string ApiKeysSectionId = "api-keys-section";
    private const string MagicLinksSectionId = "magic-links-section";
    private const string AttestationSectionId = "attestation-section";
    private const string ManuallyGeneratedAuthenticationTokensSectionId = "manually-generated-authentication-tokens-section";
    private const string AuthenticationConfigurationSectionId = "authentication-configuration-section";
    private const string DeleteApplicationSectionId = "delete-application-section";

    public SettingsTests()
    {
        Services.AddSingleton(_currentContextMock.Object);
        Services.AddSingleton(_dataServiceMock.Object);

        ComponentFactories.AddStub<PlanSection>($"<div id=\"{PlanSectionId}\"></div>");
        ComponentFactories.AddStub<ApiKeysSection>($"<div id=\"{ApiKeysSectionId}\"></div>");
        ComponentFactories.AddStub<MagicLinksSection>($"<div id=\"{MagicLinksSectionId}\"></div>");
        ComponentFactories.AddStub<AttestationSection>($"<div id=\"{AttestationSectionId}\"></div>");
        ComponentFactories.AddStub<ManuallyGeneratedAuthenticationTokensSection>($"<div id=\"{ManuallyGeneratedAuthenticationTokensSectionId}\"></div>");
        ComponentFactories.AddStub<AuthenticationConfiguration>($"<div id=\"{AuthenticationConfigurationSectionId}\"></div>");
        ComponentFactories.AddStub<DeleteApplicationSection>($"<div id=\"{DeleteApplicationSectionId}\"></div>");

        var appId = "testapp";

        _currentContextMock.SetupGet(x => x.AppId).Returns(appId);

        var application = _fixture
            .Build<Application>()
            .Without(x => x.Onboarding)
            .Without(x => x.Organization)
            .With(x => x.Id, appId)
            .Create();

        _dataServiceMock.Setup(x => x.GetApplicationAsync(It.Is<string>(p => p == appId)))
            .ReturnsAsync(application);
    }


    [InlineData(PlanSectionId)]
    [InlineData(ApiKeysSectionId)]
    [InlineData(MagicLinksSectionId)]
    [InlineData(AttestationSectionId)]
    [InlineData(ManuallyGeneratedAuthenticationTokensSectionId)]
    [InlineData(AuthenticationConfigurationSectionId)]
    [Theory]
    public void Render_DoesNotRenderPlanSection_WhenApplicationIsPendingDelete(string expectedId)
    {
        // Arrange
        _currentContextMock.SetupGet(x => x.IsPendingDelete).Returns(true);

        // Act
        var cut = Render<AdminConsole.Components.Pages.App.Settings.Settings>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find($"#{expectedId}"));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Render_AlwaysRendersDeleteApplicationSection_RespectingApplicationIsPendingDeletion(
        bool pendingDeletion)
    {
        // Arrange
        _currentContextMock.SetupGet(x => x.IsPendingDelete).Returns(pendingDeletion);

        var expectedFeatures = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        _currentContextMock.SetupGet(x => x.Features).Returns(expectedFeatures);

        // Act
        var cut = Render<AdminConsole.Components.Pages.App.Settings.Settings>();

        // Assert
        Assert.NotNull(cut.Find($"#{DeleteApplicationSectionId}"));
    }

    [Fact]
    public void Render_RendersAttestationSection_WhenAttestationIsAllowed()
    {
        // Arrange
        var expectedFeatures = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.AllowAttestation, true)
            .Create();
        _currentContextMock.SetupGet(x => x.Features).Returns(expectedFeatures);

        // Act
        var cut = Render<AdminConsole.Components.Pages.App.Settings.Settings>();

        // Assert
        Assert.NotNull(cut.Find($"#{AttestationSectionId}"));
    }

    [Fact]
    public void Render_DoesNotRenderAttestationSection_WhenAttestationIsNotAllowed()
    {
        // Arrange
        var expectedFeatures = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.AllowAttestation, false)
            .Create();
        _currentContextMock.SetupGet(x => x.Features).Returns(expectedFeatures);

        // Act
        var cut = Render<AdminConsole.Components.Pages.App.Settings.Settings>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find($"#{AttestationSectionId}"));
    }
}