using System.Security.Claims;
using System.Text;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.AdminConsole.Authorization;
using Passwordless.AdminConsole.Components.Pages.App.Settings;
using Passwordless.AdminConsole.Components.Pages.App.Settings.SettingsComponents;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.PasswordlessManagement;
using Passwordless.Common.Constants;
using Passwordless.Common.Models.Apps;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.Settings;

public class CreateSecretKeyTests : BunitContext
{
    private const string AppId = "myapp";
    private const string ApiKey = "myapp:secret:fake-key-for-tests";

    private readonly Mock<IPasswordlessManagementClient> _managementClientMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IEventLogger> _eventLoggerMock = new();
    private readonly Mock<ICreatedSecretKeyStore> _createdSecretKeyStoreMock = new();

    public CreateSecretKeyTests()
    {
        Services.AddSingleton(_managementClientMock.Object);
        Services.AddSingleton(_httpContextAccessorMock.Object);
        Services.AddSingleton(_eventLoggerMock.Object);
        Services.AddSingleton(_createdSecretKeyStoreMock.Object);
        Services.AddSingleton(new Mock<ILogger<CreatePublicKey>>().Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new(CustomClaimTypes.OrgId, "1"),
            new(ClaimTypes.NameIdentifier, "admin-id")
        }, "Test"));
        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        ComponentFactories.AddStub<CreateApiKeyComponent<SecretKeyScopes>>();
    }

    [Fact]
    public async Task OnCreatedAsync_RedirectsWithoutTheKeyInTheUrl()
    {
        // Arrange
        _managementClientMock
            .Setup(x => x.CreateApiKeyAsync(AppId, It.IsAny<CreateSecretKeyRequest>()))
            .ReturnsAsync(new CreateApiKeyResponse(ApiKey));
        var cut = Render<CreateSecretKey>(p => p.Add(x => x.AppId, AppId));

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnCreatedAsync([SecretKeyScopes.TokenRegister]));

        // Assert
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = new Uri(navigationManager.Uri);
        Assert.Equal($"/app/{AppId}/settings/secret-key-created", uri.AbsolutePath);
        Assert.Empty(uri.Query);
        Assert.Empty(uri.Fragment);
        Assert.DoesNotContain(ApiKey, navigationManager.Uri);
        Assert.DoesNotContain(Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(ApiKey)), navigationManager.Uri);
    }

    [Fact]
    public async Task OnCreatedAsync_StashesTheKeyForTheApplication()
    {
        // Arrange
        _managementClientMock
            .Setup(x => x.CreateApiKeyAsync(AppId, It.IsAny<CreateSecretKeyRequest>()))
            .ReturnsAsync(new CreateApiKeyResponse(ApiKey));
        var cut = Render<CreateSecretKey>(p => p.Add(x => x.AppId, AppId));

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnCreatedAsync([SecretKeyScopes.TokenRegister]));

        // Assert
        _createdSecretKeyStoreMock.Verify(x => x.Stash(AppId, ApiKey), Times.Once);
    }

    [Fact]
    public async Task OnCreatedAsync_DoesNotStashOrRedirect_WhenCreationFails()
    {
        // Arrange
        _managementClientMock
            .Setup(x => x.CreateApiKeyAsync(AppId, It.IsAny<CreateSecretKeyRequest>()))
            .ThrowsAsync(new HttpRequestException());
        var cut = Render<CreateSecretKey>(p => p.Add(x => x.AppId, AppId));
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var uriBefore = navigationManager.Uri;

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnCreatedAsync([SecretKeyScopes.TokenRegister]));

        // Assert
        _createdSecretKeyStoreMock.Verify(x => x.Stash(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.Equal(uriBefore, navigationManager.Uri);
    }
}