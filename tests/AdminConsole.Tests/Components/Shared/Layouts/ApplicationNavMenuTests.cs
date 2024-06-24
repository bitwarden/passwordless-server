using System.Collections.Immutable;
using AutoFixture;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Components.Layouts;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Layouts;

public class ApplicationNavMenuTests : TestContext
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ICurrentContext> _currentContext = new();
    private readonly Mock<IWebHostEnvironment> _webHostEnvironment = new();

    private readonly TestAuthorizationContext _authorizationContext;

    private readonly Fixture _fixture = new();

    public ApplicationNavMenuTests()
    {
        Services.AddSingleton(_httpContextAccessorMock.Object);
        Services.AddSingleton(_currentContext.Object);
        Services.AddSingleton(_webHostEnvironment.Object);
        _authorizationContext = this.AddTestAuthorization();
        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(
            new DefaultHttpContext
            {
                Items = new Dictionary<object, object?>()
            });
    }

    [Fact]
    public void ApplicationNavMenu_Renders_ExpectedApplicationLinks_ApplicationIsNotPendingDeletion()
    {
        // Arrange
        var organizationFeatures = _fixture
            .Build<OrganizationFeaturesContext>()
            .With(x => x.EventLoggingIsEnabled, true)
            .Create();
        _currentContext.SetupGet(x => x.OrganizationFeatures).Returns(organizationFeatures);
        var features = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.EventLoggingIsEnabled, true)
            .Create();
        _currentContext.SetupGet(x => x.Features).Returns(features);

        var organization = _fixture.Build<Organization>()
            .Without(x => x.Applications)
            .Without(x => x.Admins)
            .Create();
        _currentContext.SetupGet(x => x.Organization).Returns(organization);
        _currentContext.SetupGet(x => x.IsPendingDelete).Returns(false);
        _currentContext.SetupGet(x => x.InAppContext).Returns(true);
        _authorizationContext.SetAuthorized("John Doe");
        _authorizationContext.SetPolicies(CustomPolicy.HasAppRole);

        // Act
        var cut = RenderComponent<ApplicationNavMenu>();

        // Assert
        var submenu = cut.Find("div[id=\"app-submenu\"]");
        var links = submenu.Children.Where(x => x.ClassName == "nav-link").ToImmutableList();
        Assert.Equal(6, links.Count);
        Assert.Contains("Getting Started", links[0].TextContent);
        Assert.Contains("Playground", links[1].TextContent);
        Assert.Contains("Users", links[2].TextContent);
        Assert.Contains("Reporting", links[3].TextContent);
        Assert.Contains("App Logs", links[4].TextContent);
        Assert.Contains("Settings", links[5].TextContent);
    }

    [Fact]
    public void ApplicationNavMenu_Renders_ExpectedApplicationLinks_ApplicationIsPendingDeletion()
    {
        // Arrange
        var organizationFeatures = _fixture
            .Build<OrganizationFeaturesContext>()
            .With(x => x.EventLoggingIsEnabled, true)
            .Create();
        _currentContext.SetupGet(x => x.OrganizationFeatures).Returns(organizationFeatures);
        var features = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.EventLoggingIsEnabled, true)
            .Create();
        _currentContext.SetupGet(x => x.Features).Returns(features);

        var organization = _fixture.Build<Organization>()
            .Without(x => x.Applications)
            .Without(x => x.Admins)
            .Create();
        _currentContext.SetupGet(x => x.Organization).Returns(organization);
        _currentContext.SetupGet(x => x.IsPendingDelete).Returns(true);
        _currentContext.SetupGet(x => x.InAppContext).Returns(true);
        _authorizationContext.SetAuthorized("John Doe");
        _authorizationContext.SetPolicies(CustomPolicy.HasAppRole);

        // Act
        var cut = RenderComponent<ApplicationNavMenu>();

        // Assert
        var submenu = cut.Find("div[id=\"app-submenu\"]");
        var links = submenu.Children.Where(x => x.ClassName == "nav-link").ToImmutableList();
        Assert.Single(links);
        Assert.Contains("Settings", links[0].TextContent);
    }

    [Fact]
    public void ApplicationNavMenu_Renders_Nothing_When_HasAppRolePolicyFails()
    {
        // Arrange
        var organizationFeatures = _fixture
            .Build<OrganizationFeaturesContext>()
            .With(x => x.EventLoggingIsEnabled, true)
            .Create();
        _currentContext.SetupGet(x => x.OrganizationFeatures).Returns(organizationFeatures);
        var features = _fixture
            .Build<ApplicationFeatureContext>()
            .With(x => x.EventLoggingIsEnabled, true)
            .Create();
        _currentContext.SetupGet(x => x.Features).Returns(features);

        var organization = _fixture.Build<Organization>()
            .Without(x => x.Applications)
            .Without(x => x.Admins)
            .Create();
        _currentContext.SetupGet(x => x.Organization).Returns(organization);
        _currentContext.SetupGet(x => x.IsPendingDelete).Returns(true);
        _currentContext.SetupGet(x => x.InAppContext).Returns(true);
        _authorizationContext.SetNotAuthorized();

        // Act
        var cut = RenderComponent<ApplicationNavMenu>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("div[id=\"app-submenu\"]"));
    }
}