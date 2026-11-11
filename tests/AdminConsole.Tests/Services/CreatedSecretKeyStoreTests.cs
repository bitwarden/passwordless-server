using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Services;

/// <summary>
/// Exercises <see cref="CreatedSecretKeyStore" /> against the real cookie TempData provider, simulating the POST that
/// creates the key and the GET that displays it as two separate requests that only share cookies.
/// </summary>
public class CreatedSecretKeyStoreTests : IDisposable
{
    private const string AppId = "myapp";
    private const string ApiKey = "myapp:secret:fake-key-for-tests";

    private readonly ServiceProvider _serviceProvider;

    public CreatedSecretKeyStoreTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddMvcCore().AddViews();
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose() => _serviceProvider.Dispose();

    [Fact]
    public void Take_ReturnsStashedKey_OnTheNextRequest()
    {
        // Arrange
        var createRequest = CreateHttpContext();
        CreateSut(createRequest).Stash(AppId, ApiKey);

        // Act
        var displayRequest = CreateHttpContext(GetCookies(createRequest));
        var actual = CreateSut(displayRequest).Take(AppId);

        // Assert
        Assert.Equal(ApiKey, actual);
    }

    [Fact]
    public void Stash_DoesNotExposeKey_InResponseHeaders()
    {
        // Arrange
        var createRequest = CreateHttpContext();

        // Act
        CreateSut(createRequest).Stash(AppId, ApiKey);

        // Assert
        var responseHeaders = string.Join('\n', createRequest.Response.Headers.Select(h => $"{h.Key}: {h.Value}"));
        Assert.NotEqual(0, createRequest.Response.Headers.SetCookie.Count);
        Assert.DoesNotContain(ApiKey, responseHeaders);
        Assert.DoesNotContain(Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(ApiKey)), responseHeaders);
        Assert.DoesNotContain(Convert.ToBase64String(Encoding.UTF8.GetBytes(ApiKey)), responseHeaders);
        Assert.DoesNotContain("fake-key-for-tests", responseHeaders);
    }

    [Fact]
    public void Take_ReturnsNull_OnceTheKeyHasBeenShown()
    {
        // Arrange
        var createRequest = CreateHttpContext();
        CreateSut(createRequest).Stash(AppId, ApiKey);
        var browserCookies = GetCookies(createRequest);
        var displayRequest = CreateHttpContext(browserCookies);
        CreateSut(displayRequest).Take(AppId);

        // Act
        var refreshRequest = CreateHttpContext(GetCookies(displayRequest, browserCookies));
        var actual = CreateSut(refreshRequest).Take(AppId);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void Take_ReturnsNull_WhenNothingWasStashed()
    {
        // Arrange
        var displayRequest = CreateHttpContext();

        // Act
        var actual = CreateSut(displayRequest).Take(AppId);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void Take_ReturnsNull_ForADifferentApplication_AndLeavesTheKeyInPlace()
    {
        // Arrange
        var createRequest = CreateHttpContext();
        CreateSut(createRequest).Stash(AppId, ApiKey);

        // Act
        var otherAppRequest = CreateHttpContext(GetCookies(createRequest));
        var otherAppActual = CreateSut(otherAppRequest).Take("otherapp");

        var displayRequest = CreateHttpContext(GetCookies(otherAppRequest, GetCookies(createRequest)));
        var actual = CreateSut(displayRequest).Take(AppId);

        // Assert
        Assert.Null(otherAppActual);
        Assert.Equal(ApiKey, actual);
    }

    [Fact]
    public void Take_ReturnsNull_WhenTheCookieHasBeenTamperedWith()
    {
        // Arrange
        var createRequest = CreateHttpContext();
        CreateSut(createRequest).Stash(AppId, ApiKey);
        var tampered = GetCookies(createRequest)
            .ToDictionary(c => c.Key, _ => Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes($"{AppId}:secret:attacker-chosen")));

        // Act
        var displayRequest = CreateHttpContext(tampered);
        var actual = CreateSut(displayRequest).Take(AppId);

        // Assert
        Assert.Null(actual);
    }

    private CreatedSecretKeyStore CreateSut(HttpContext httpContext) =>
        new(new HttpContextAccessor { HttpContext = httpContext },
            _serviceProvider.GetRequiredService<ITempDataDictionaryFactory>());

    private DefaultHttpContext CreateHttpContext(IReadOnlyDictionary<string, string>? cookies = null)
    {
        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("admin.passwordless.dev");

        if (cookies is { Count: > 0 })
        {
            httpContext.Request.Headers.Cookie = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));
        }

        return httpContext;
    }

    /// <summary>
    /// Applies the response's Set-Cookie headers on top of the cookies the browser already holds, the way a browser
    /// carries cookies from one request to the next.
    /// </summary>
    private static Dictionary<string, string> GetCookies(HttpContext httpContext,
        IReadOnlyDictionary<string, string>? existing = null)
    {
        var cookies = existing?.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();

        foreach (var header in SetCookieHeaderValue.ParseList(httpContext.Response.Headers.SetCookie.ToList()))
        {
            var isDeleted = header.Expires is { } expires && expires < DateTimeOffset.UtcNow;
            if (isDeleted || header.Value.Length == 0)
            {
                cookies.Remove(header.Name.Value!);
            }
            else
            {
                cookies[header.Name.Value!] = header.Value.Value!;
            }
        }

        return cookies;
    }
}