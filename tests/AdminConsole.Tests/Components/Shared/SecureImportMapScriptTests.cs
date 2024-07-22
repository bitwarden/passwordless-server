using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Shared;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared;

public class SecureImportMapScriptTests : TestContext
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IFileVersionProvider> _fileVersionProviderMock = new();

    public SecureImportMapScriptTests()
    {
        this.Services.AddSingleton(_httpContextAccessorMock.Object);
        this.Services.AddSingleton(_fileVersionProviderMock.Object);
    }

    [Fact]
    public void SecureImportMapScript_Renders_NonceAttribute()
    {
        // Arrange
        var value = new SecureImportMapScript.Model
        {
            Imports = new Dictionary<string, string>
            {
                { "vue", "/lib/vue/vue.mjs" }
            }
        };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            Items = new Dictionary<object, object?>()
            {
                { "csp-nonce", "test-nonce" }
            }
        });
        _fileVersionProviderMock.Setup(x => x.AddFileVersionToPath("", "/lib/vue/vue.mjs")).Returns("/lib/vue/vue.mjs?v=1");

        // Act
        var cut = RenderComponent<SecureImportMapScript>(p =>
            p.Add(x => x.Value, value));

        // Assert
        cut.MarkupMatches("<script type=\"importmap\" nonce=\"test-nonce\" diff:ignoreChildren>{\"imports\":{\"vue\":\"/lib/vue/vue.mjs?v=1\"}}</script>");
    }

    [Fact]
    public void SecureImportMapScript_Renders_WithoutNonceAttribute()
    {
        // Arrange
        var value = new SecureImportMapScript.Model
        {
            Imports = new Dictionary<string, string>
            {
                { "vue", "/lib/vue/vue.mjs" }
            }
        };
        _fileVersionProviderMock.Setup(x => x.AddFileVersionToPath("", "/lib/vue/vue.mjs")).Returns("/lib/vue/vue.mjs?v=1");

        // Act
        var cut = RenderComponent<SecureImportMapScript>(p =>
            p.Add(x => x.Value, value));

        // Assert
        cut.MarkupMatches("<script type=\"importmap\" diff:ignoreChildren>{\"imports\":{\"vue\":\"/lib/vue/vue.mjs?v=1\"}}</script>");
    }

    [Fact]
    public void SecureImportMapScript_Renders_InlineScript()
    {
        // Arrange
        var value = new SecureImportMapScript.Model
        {
            Imports = new Dictionary<string, string>
            {
                { "vue", "/lib/vue/vue.mjs" }
            }
        };
        _fileVersionProviderMock.Setup(x => x.AddFileVersionToPath("", "/lib/vue/vue.mjs")).Returns("/lib/vue/vue.mjs?v=1");

        // Act
        var cut = RenderComponent<SecureImportMapScript>(parameters => parameters
            .Add(p => p.Value, value));

        // Assert
        cut.MarkupMatches("<script type=\"importmap\">{\"imports\":{\"vue\":\"/lib/vue/vue.mjs?v=1\"}}</script>");
    }
}