using AutoFixture;
using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Pages.Organization;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.Organization;

public class OverviewTests : BunitContext
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IDataService> _dataServiceMock = new();
    private static readonly Models.Organization _validOrganization = new()
    {
        Id = 1,
        Name = "Test Organization",
        Applications = new List<Application>
        {
            new()
            {
                Id = "appid1",
                Name = "Application 1",
                Description = "Application 1 Description",
                CurrentUserCount = 12,
                CreatedAt = new DateTime(2023, 1, 1),
                DeleteAt = new DateTime(2024, 5, 1)
            },
            new()
            {
                Id = "appid2",
                Name = "Application 2",
                Description = "Application 2 Description",
                CurrentUserCount = 52,
                CreatedAt = new DateTime(2024, 1, 1)
            },
        }
    };
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IFileVersionProvider> _fileVersionProviderMock = new();

    public OverviewTests()
    {
        Services.AddSingleton(_dataServiceMock.Object);

        var items = new Dictionary<object, object> { ["csp-nonce"] = "mocked-nonce-value" };

        // Set up the mock to return a mock HttpContext with the desired Items property
        _httpContextAccessorMock.SetupGet(x => x.HttpContext.Items).Returns(items);
        Services.AddSingleton(_httpContextAccessorMock.Object);
        Services.AddSingleton(_fileVersionProviderMock.Object);
    }

    [Fact]
    public void Renders_CreateApplicationButton_WhenCanCreateMoreApplications()
    {
        // Arrange
        _dataServiceMock.Setup(x => x.AllowedToCreateApplicationAsync()).ReturnsAsync(true);
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(_validOrganization);

        // Act
        var cut = Render<Overview>();

        // Assert
        var actual = cut.Find("#create-application-btn");
        Assert.NotNull(actual);
        Assert.Throws<ElementNotFoundException>(() => cut.Find("#upgrade-organization-alert"));
    }

    [Fact]
    public void Renders_UpgradeAlert_WhenCannotCreateMoreApplications()
    {
        // Arrange
        _dataServiceMock.Setup(x => x.AllowedToCreateApplicationAsync()).ReturnsAsync(false);
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(_validOrganization);

        // Act
        var cut = Render<Overview>();

        // Assert
        var actual = cut.Find("#upgrade-organization-alert");
        Assert.NotNull(actual);
        Assert.Throws<ElementNotFoundException>(() => cut.Find("#create-application-btn"));
    }

    [Fact]
    public void Renders_ExpectedPageTitle()
    {
        // Arrange
        _dataServiceMock.Setup(x => x.AllowedToCreateApplicationAsync()).ReturnsAsync(true);
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(_validOrganization);

        // Act
        var cut = Render<Overview>();

        // Assert
        var actual = cut.Find("h1");
        Assert.Equal("Test Organization Applications", actual.TextContent);
    }

    [Fact]
    public void Renders_ExpectedApplications()
    {
        // Arrange
        _dataServiceMock.Setup(x => x.AllowedToCreateApplicationAsync()).ReturnsAsync(true);
        _dataServiceMock.Setup(x => x.GetOrganizationWithDataAsync()).ReturnsAsync(_validOrganization);

        // Act
        var cut = Render<Overview>();

        // Assert
        var actualListItem1 = cut.Find("#appid1-list-item");
        Assert.NotNull(actualListItem1);

        var actualListItem2 = cut.Find("#appid2-list-item");
        Assert.NotNull(actualListItem2);
    }
}