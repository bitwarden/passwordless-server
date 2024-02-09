using Bunit;
using Microsoft.AspNetCore.Http;
using Moq;
using Passwordless.AdminConsole.Components.Shared;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared;

public class SecureScriptTests : TestContext
{
    [Fact]
    public void SecureScript_Renders_NonceAttribute()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            Items = new Dictionary<object, object?>()
            {
                { "csp-nonce", "test-nonce" }
            }
        });

        // Act
        var cut = RenderComponent<SecureScript>();

        // Assert
        cut.MarkupMatches($"<script nonce=\"test-nonce\" diff:ignoreChildren></script>");
    }
}