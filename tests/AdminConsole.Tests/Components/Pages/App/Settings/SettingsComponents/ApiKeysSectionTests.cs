using System.Collections.Immutable;
using AutoFixture;
using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Models.Apps;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Settings.SettingsComponents;

public class ApiKeysSectionTests : TestContext
{
    private readonly Mock<ICurrentContext> _currentContext = new();
    private readonly Mock<IPasswordlessManagementClient> _managementClientMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IEventLogger> _eventLoggerMock = new();
    private readonly Mock<ILogger<ApiKeysSection>> _loggerMock = new();
    private readonly Mock<IFileVersionProvider> _fileVersionProviderMock = new();

    private readonly Fixture _fixture = new();

    public ApiKeysSectionTests()
    {
        Services.AddSingleton(_currentContext.Object);
        Services.AddSingleton(_managementClientMock.Object);
        Services.AddSingleton(_httpContextAccessorMock.Object);
        Services.AddSingleton(_fileVersionProviderMock.Object);

        var httpContextItems = new Dictionary<object, object> { ["csp-nonce"] = "mocked-nonce-value" };
        _httpContextAccessorMock.SetupGet(x => x.HttpContext)
            .Returns(new DefaultHttpContext { Items = httpContextItems });

        Services.AddSingleton(_eventLoggerMock.Object);
        Services.AddSingleton(_loggerMock.Object);
    }

    [Fact]
    public void ApiKeysSection_DoesNotRender_WhenApplicationIsPendingDeletion()
    {
        // Arrange
        _currentContext.SetupGet(x => x.IsPendingDelete).Returns(true);
        var cut = RenderComponent<ApiKeysSection>();

        // Assert
        cut.MarkupMatches("");
        _managementClientMock.Verify(x => x.GetApiKeysAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ApiKeysSection_Renders_WhenApplicationIsNotPendingDeletion()
    {
        // Arrange
        _currentContext.SetupGet(x => x.IsPendingDelete).Returns(false);
        _currentContext.SetupGet(x => x.AppId).Returns("myapp");
        _currentContext.SetupGet(x => x.ApiKey).Returns("myapikey");
        _currentContext.SetupGet(x => x.ApiSecret).Returns("myapisecret");
        var expectedApiKeys = _fixture.CreateMany<ApiKeyResponse>().ToImmutableList();
        _managementClientMock.Setup(x => x.GetApiKeysAsync(It.Is<string>(p => p == _currentContext.Object.AppId!)))
            .ReturnsAsync(expectedApiKeys);
        var cut = RenderComponent<ApiKeysSection>();

        // Assert
        cut.MarkupMatches("<div class=\"panel\" diff:ignoreChildren></div>");
    }

    [Fact]
    public void ApiKeysSection_Renders_LockIcon_WhenApiKeyIsUnlocked()
    {
        // Arrange
        _currentContext.SetupGet(x => x.IsPendingDelete).Returns(false);
        _currentContext.SetupGet(x => x.AppId).Returns("myapp");
        _currentContext.SetupGet(x => x.ApiKey).Returns("myapikey");
        _currentContext.SetupGet(x => x.ApiSecret).Returns("myapisecret");
        var expectedApiKeys = new List<ApiKeyResponse>
        {
            _fixture.Build<ApiKeyResponse>().With(x => x.IsLocked, false).Create()
        };
        _managementClientMock.Setup(x => x.GetApiKeysAsync(It.Is<string>(p => p == _currentContext.Object.AppId!)))
            .ReturnsAsync(expectedApiKeys);
        var cut = RenderComponent<ApiKeysSection>();

        // Assert
        var actualCell = cut.FindAll("tbody>tr").Single(x => x.Children.Length > 1).Children.Last();
        var actualDiv = actualCell.Children.First(x => x.NodeName == "DIV");
        var actual = actualDiv.Children.Single(x => x.NodeName == "BUTTON");
        Assert.Contains(actual.Attributes, x => x.Name == "value" && x.Value == expectedApiKeys.First().Id);
        actual.MarkupMatches("<button diff:ignoreAttributes><svg diff:ignore />Lock</button>");
    }

    [Fact]
    public void ApiKeysSection_Renders_ExpectedIcons_WhenApiKeyIsLocked()
    {
        // Arrange
        _currentContext.SetupGet(x => x.IsPendingDelete).Returns(false);
        _currentContext.SetupGet(x => x.AppId).Returns("myapp");
        _currentContext.SetupGet(x => x.ApiKey).Returns("myapikey");
        _currentContext.SetupGet(x => x.ApiSecret).Returns("myapisecret");
        var expectedApiKeys = new List<ApiKeyResponse>
        {
            _fixture.Build<ApiKeyResponse>().With(x => x.IsLocked, true).Create()
        };
        _managementClientMock.Setup(x => x.GetApiKeysAsync(It.Is<string>(p => p == _currentContext.Object.AppId!)))
            .ReturnsAsync(expectedApiKeys);
        var cut = RenderComponent<ApiKeysSection>();

        // Assert
        var actualCell = cut.FindAll("tbody>tr").Single(x => x.Children.Length > 1).Children.Last();
        var actualDiv = actualCell.Children.First(x => x.NodeName == "DIV");
        var actualUnlockButton = actualDiv.Children.First(x => x.NodeName == "BUTTON");
        Assert.Contains(actualUnlockButton.Attributes, x => x.Name == "value" && x.Value == expectedApiKeys.First().Id);
        actualUnlockButton.MarkupMatches("<button diff:ignoreAttributes><svg diff:ignore />Unlock</button>");
        var actualDeleteButton = actualDiv.Children.Last(x => x.NodeName == "BUTTON");
        Assert.Contains(actualDeleteButton.Attributes, x => x.Name == "value" && x.Value == expectedApiKeys.First().Id);
        actualDeleteButton.MarkupMatches("<button diff:ignoreAttributes><svg diff:ignore />Delete</button>");
    }
}