using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Shared;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared;

public class SecureScriptTests : TestContext
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IFileVersionProvider> _fileVersionProviderMock = new();

    public SecureScriptTests()
    {
        this.Services.AddSingleton(_httpContextAccessorMock.Object);
        this.Services.AddSingleton(_fileVersionProviderMock.Object);
    }

    [Fact]
    public void SecureScript_Renders_NonceAttribute()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            Items = new Dictionary<object, object?>()
            {
                { "csp-nonce", "test-nonce" }
            }
        });

        // Act
        var cut = RenderComponent<SecureScript>();

        // Assert
        cut.MarkupMatches("<script nonce=\"test-nonce\" diff:ignoreChildren></script>");
    }

    [Fact]
    public void SecureScript_Renders_WithoutNonceAttribute()
    {
        // Arrange

        // Act
        var cut = RenderComponent<SecureScript>();

        // Assert
        cut.MarkupMatches("<script diff:ignoreChildren></script>");
    }

    [Fact]
    public void SecureScript_Renders_InlineScript()
    {
        // Arrange

        // Act
        var cut = RenderComponent<SecureScript>(parameters => parameters
            .Add(p => p.ChildContent, "console.log('Hello, World!');"));

        // Assert
        cut.MarkupMatches("<script diff:ignoreAttributes>console.log('Hello, World!');</script>");
    }

    [Fact]
    public void SecureScript_Renders_ScriptFile()
    {
        // Arrange
        _fileVersionProviderMock.Setup(x => x.AddFileVersionToPath("", "test.js")).Returns("test.js?v=1");
        var additionalAttributes = new Dictionary<string, object>
        {
            { "src", "test.js" },
            { "type", "module" }
        };

        // Act
        var cut = RenderComponent<SecureScript>(parameters => parameters
            .Add(p => p.AdditionalAttributes, additionalAttributes));

        // Assert
        cut.MarkupMatches("<script src=\"test.js?v=1\" type=\"module\" diff:ignoreChildren></script>");
    }
}