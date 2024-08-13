using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.AdminConsole.Components.Pages.Organization;
using Passwordless.AdminConsole.Components.Pages.Organization.SettingsComponents;
using Passwordless.AdminConsole.FeatureManagement;
using Passwordless.Common.Services.Mail;
using Passwordless.Common.Services.Mail.File;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.Organization;

public class VerifyTests : TestContext
{
    private readonly Mock<IOptionsSnapshot<MailConfiguration>> _mailOptionsMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock = new();

    public VerifyTests()
    {
        Services.AddSingleton(_mailOptionsMock.Object);
        Services.AddSingleton(_httpContextAccessorMock.Object);
        Services.AddSingleton(_webHostEnvironmentMock.Object);
    }

    [Fact]
    public void Render_Renders_FileProviderDebugSection_WhenInDevelopment()
    {
        // Arrange
        _webHostEnvironmentMock.SetupGet(x => x.EnvironmentName).Returns(Environments.Development);
        var mailConfiguration = new MailConfiguration()
        {
            From = "",
            Providers = new List<BaseProviderOptions>()
            {
                new FileProviderOptions { Name = "File", Path = "/usr/local/var/mail" }
            }
        };
        _mailOptionsMock.SetupGet(x => x.Value).Returns(mailConfiguration);

        // Act
        var cut = RenderComponent<Verify>();

        // Assert
        Assert.NotNull(cut.Find("#file-provider-debug-section"));
    }

    [Fact]
    public void Render_Renders_FileProviderDebugSection_WhenNotInDevelopment()
    {
        // Arrange
        _webHostEnvironmentMock.SetupGet(x => x.EnvironmentName).Returns(Environments.Production);
        var mailConfiguration = new MailConfiguration()
        {
            From = "",
            Providers = new List<BaseProviderOptions>()
            {
                new FileProviderOptions { Name = "File", Path = "/usr/local/var/mail" }
            }
        };
        _mailOptionsMock.SetupGet(x => x.Value).Returns(mailConfiguration);

        // Act
        var cut = RenderComponent<Verify>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("#file-provider-debug-section"));
    }
}