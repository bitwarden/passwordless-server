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
using Passwordless.Common.Constants;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Settings.SettingsComponents;

public class CreateApiKeyComponentTests : BunitContext
{
    private readonly Mock<ICurrentContext> _currentContext = new();
    private readonly Mock<IPasswordlessManagementClient> _managementClientMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IEventLogger> _eventLoggerMock = new();
    private readonly Mock<ILogger<ApiKeysSection>> _loggerMock = new();
    private readonly Mock<IFileVersionProvider> _fileVersionProviderMock = new();

    private readonly Fixture _fixture = new();

    public CreateApiKeyComponentTests()
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
    public void CreateApiKeyComponent_Renders_ExpectedScopes()
    {
        // Arrange
        var scopes = Enum.GetValues<SecretKeyScopes>().ToList();
        var cut = Render<CreateApiKeyComponent<SecretKeyScopes>>(p => p
            .Add(x => x.Scopes, scopes)
            .Add(x => x.OnCreateClicked, () => { }));

        // Assert
        var forms = cut.FindAll("form");
        Assert.Single(forms);
        var form = forms[0];

        var scopeDivs = form.Children.Where(controlGroup => controlGroup.NodeName == "DIV" && controlGroup.Children.Any(control => control.NodeName == "INPUT")).ToList();
        Assert.Equal(scopes.Count, scopeDivs.Count);

        Assert.Contains("token_register", scopeDivs.ElementAt(0).QuerySelector("label")!.TextContent);
        Assert.Contains("token_verify", scopeDivs.ElementAt(1).QuerySelector("label")!.TextContent);
    }
}