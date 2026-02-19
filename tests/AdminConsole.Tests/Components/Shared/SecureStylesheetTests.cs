using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Shared;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared;

public class SecureStylesheetTests : BunitContext
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IFileVersionProvider> _fileVersionProviderMock = new();

    public SecureStylesheetTests()
    {
        this.Services.AddSingleton(_httpContextAccessorMock.Object);
        this.Services.AddSingleton(_fileVersionProviderMock.Object);
    }

    [Fact]
    public void SecureStylesheet_DoesNot_NonceAttributeForCssFile()
    {
        // Arrange
        _fileVersionProviderMock.Setup(x => x.AddFileVersionToPath(string.Empty, "test.css")).Returns("test.css?v=1");
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            Items = new Dictionary<object, object?>()
            {
                { "csp-nonce", "test-nonce" }
            }
        });
        var additionalAttributes = new Dictionary<string, object>
        {
            { "href", "test.css" }
        };

        // Act
        var cut = Render<SecureStylesheet>(parameters => parameters
            .Add(p => p.AdditionalAttributes, additionalAttributes));

        // Assert
        cut.MarkupMatches("<link rel=\"stylesheet\" href=\"test.css?v=1\" diff:ignoreChildren></script>");
    }

    [Fact]
    public void SecureStylesheet_Renders_NonceAttributeForInlineCss()
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
        var cut = Render<SecureStylesheet>(parameters => parameters
            .Add(p => p.ChildContent, "body { color: red; }"));


        // Assert
        cut.MarkupMatches("<style nonce=\"test-nonce\">body { color: red; }</style>");
    }

    [Fact]
    public void SecureStylesheet_Renders_LinkTag_ForFile()
    {
        // Arrange
        _fileVersionProviderMock.Setup(x => x.AddFileVersionToPath(string.Empty, "test.css")).Returns("test.css?v=1");
        var additionalAttributes = new Dictionary<string, object>
        {
            { "href", "test.css" }
        };

        // Act
        var cut = Render<SecureStylesheet>(parameters => parameters
            .Add(p => p.AdditionalAttributes, additionalAttributes));

        // Assert
        cut.MarkupMatches("<link rel=\"stylesheet\" href=\"test.css?v=1\" diff:ignoreChildren></script>");
    }

    [Fact]
    public void SecureStylesheet_Renders_LinkTag_ForInlineCss()
    {
        // Arrange

        // Act
        var cut = Render<SecureStylesheet>(parameters => parameters
            .Add(p => p.ChildContent, "body { color: red; }"));

        // Assert
        cut.MarkupMatches("<style diff:ignoreAttributes>body { color: red; }</style>");
    }
}