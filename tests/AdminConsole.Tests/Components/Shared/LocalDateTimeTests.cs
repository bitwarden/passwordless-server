using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Shared;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared;

public class LocalDateTimeTests : TestContext
{
    [Fact]
    public void LocalDateTime_Renders_ExpectedMarkup()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            Items = new Dictionary<object, object?> { { "csp-nonce", "hash" } }
        });

        Services.AddSingleton(httpContextAccessorMock.Object);
        var cut = RenderComponent<LocalDateTime>(parameters => parameters
            .Add(p => p.Value, new DateTime(1990, 01, 05, 18, 0, 0)));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Contains($"<time id=\"{cut.Instance.Id}\" datetime=\"1990-01-05T18:00:00\"></time>", actual);
        Assert.Contains($"document.getElementById(\"{cut.Instance.Id}\").innerText = new Date(\"1990-01-05T18:00:00\").toLocaleString();", actual);
    }

    [Fact]
    public void LocalDateTime_Applies_Nonce_OnScriptTag()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            Items = new Dictionary<object, object?> { { "csp-nonce", "hash" } }
        });

        Services.AddSingleton(httpContextAccessorMock.Object);
        var cut = RenderComponent<LocalDateTime>(parameters => parameters
            .Add(p => p.Value, new DateTime(1990, 01, 05, 18, 0, 0)));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Contains($"<script nonce=\"hash\">", actual);
    }
}