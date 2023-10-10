using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Passwordless.Common.Middleware.SelfHosting;

namespace Passwordless.Common.Tests.Middleware.SelfHosting;

public class HttpOverridesMiddlewareTests
{

    [Fact]
    public async Task InvokeAsync_Modifies_Scheme_WhenXForwardedProtoHeaderNotPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new HttpOverridesMiddleware(next: (innerHttpContext) => Task.CompletedTask);
        context.Request.Headers["X-Forwarded-Proto"] = new StringValues("https");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("https", context.Request.Scheme);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotModify_Scheme_WhenXForwardedProtoHeaderNotPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        var middleware = new HttpOverridesMiddleware(next: (innerHttpContext) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("http", context.Request.Scheme);
    }

    // You can add more test cases as needed to cover different scenarios.
}