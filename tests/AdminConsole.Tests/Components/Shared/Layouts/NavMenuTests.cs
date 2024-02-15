using System.Collections.Immutable;
using AutoFixture;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Layouts;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Layouts;

public class NavMenuTests : TestContext
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ICurrentContext> _currentContext = new();
    private readonly Mock<IWebHostEnvironment> _webHostEnvironment = new();

    private readonly TestAuthorizationContext _authorizationContext;

    private readonly Fixture _fixture = new();

    public NavMenuTests()
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
    public void NavMenu_Renders_EmptySidebar_WhenNotAuthenticated()
    {
        // Arrange

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        Assert.False(cut.Find("nav").Children.Any());
    }

    [Fact]
    public void NavMenu_Renders_Sidebar_WhenAuthenticated()
    {
        // Arrange
        var organizationFeatures = _fixture.Create<OrganizationFeaturesContext>();
        _currentContext.SetupGet(x => x.OrganizationFeatures).Returns(organizationFeatures);
        var organization = _fixture.Build<Organization>()
            .Without(x => x.Applications)
            .Without(x => x.Admins)
            .Create();
        _currentContext.SetupGet(x => x.Organization).Returns(organization);
        _authorizationContext.SetAuthorized("jonas");

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        Assert.True(cut.Find("nav").Children.Any());
    }

    [Fact]
    public void NavMenu_Renders_ExpectedApplicationLinks_ApplicationIsPendingDeletion()
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
        _authorizationContext.SetAuthorized("jonas");

        // Act
        var cut = RenderComponent<NavMenu>();

        // Assert
        var submenu = cut.Find("div[id=\"app-submenu\"]");
        var links = submenu.Children.Where(x => x.ClassName == "nav-link").ToImmutableList();
        Assert.Equal(2, links.Count);
        Assert.Contains("Settings", links[0].TextContent);
        Assert.Contains("App Logs", links[1].TextContent);
    }
}