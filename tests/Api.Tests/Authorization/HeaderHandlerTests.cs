using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Passwordless.Api.Authorization;

#nullable enable

namespace Passwordless.Api.Tests.Authorization;

public class HeaderHandlerTests
{
    private readonly Mock<IOptionsMonitor<HeaderOptions<object>>> _mockOptions;
    private readonly Mock<UrlEncoder> _mockUrlEncoder;
    private readonly Mock<ISystemClock> _mockClock;
    private readonly Mock<IProblemDetailsService> _mockProblemDetailsService;

    private readonly HeaderHandler<object> _sut;

    public HeaderHandlerTests()
    {
        _mockOptions = new Mock<IOptionsMonitor<HeaderOptions<object>>>();
        _mockUrlEncoder = new Mock<UrlEncoder>();
        _mockClock = new Mock<ISystemClock>();
        _mockProblemDetailsService = new Mock<IProblemDetailsService>();

        _sut = new HeaderHandler<object>(
            _mockOptions.Object,
            NullLoggerFactory.Instance,
            _mockUrlEncoder.Object,
            _mockClock.Object,
            _mockProblemDetailsService.Object,
            new object());
    }

    [Fact]
    public async Task HandleAuthenticateAsync_HeaderIsValid_Succeeds()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["ApiKey"] = "test_key";

        _mockOptions.Setup(o => o.Get("Test"))
            .Returns(new HeaderOptions<object>
            {
                HeaderName = "ApiKey",
                ClaimsCreator = (dep, header) =>
                {
                    Assert.Equal("test_key", header);

                    return Task.FromResult(Array.Empty<Claim>());
                },
            });

        await _sut.InitializeAsync(
            new AuthenticationScheme("Test", null, typeof(HeaderHandler<object>)),
            context);

        var result = await _sut.AuthenticateAsync();

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_HeaderDoesNotExist_ReturnsNoResult()
    {
        var context = new DefaultHttpContext();

        _mockOptions.Setup(o => o.Get("Test"))
            .Returns(new HeaderOptions<object>
            {
                HeaderName = "ApiKey",
                ClaimsCreator = (dep, header) =>
                {
                    Assert.Fail("ClaimsCreator should not have ran.");
                    return Task.FromResult(Array.Empty<Claim>());
                },
            });

        await _sut.InitializeAsync(
            new AuthenticationScheme("Test", null, typeof(HeaderHandler<object>)),
            context);

        var result = await _sut.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_HeaderIsEmpty_ReturnsNoResult()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["ApiKey"] = StringValues.Empty;

        _mockOptions.Setup(o => o.Get("Test"))
            .Returns(new HeaderOptions<object>
            {
                HeaderName = "ApiKey",
                ClaimsCreator = (dep, header) =>
                {
                    Assert.Fail("ClaimsCreator should not have ran.");
                    return Task.FromResult(Array.Empty<Claim>());
                },
            });

        await _sut.InitializeAsync(
            new AuthenticationScheme("Test", null, typeof(HeaderHandler<object>)),
            context);

        var result = await _sut.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleChallengeAsync_WritesDetailError()
    {
        var context = new DefaultHttpContext();

        _mockOptions.Setup(o => o.Get("Test"))
            .Returns(new HeaderOptions<object>
            {
                HeaderName = "ApiKey",
                ClaimsCreator = (dep, header) =>
                {
                    Assert.Fail("ClaimsCreator should not have ran.");
                    return Task.FromResult(Array.Empty<Claim>());
                },
                ProblemDetailWriter = new TestProblemDetailWriter("Test Error 1", "Test Error 2")
            });

        await _sut.InitializeAsync(
            new AuthenticationScheme("Test", null, typeof(HeaderHandler<object>)),
            context);

        await _sut.ChallengeAsync(null);

        Assert.Equal("Test", context.Response.Headers[HeaderNames.WWWAuthenticate].Single());
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);

        // The problem details title should hint towards the right header
        _mockProblemDetailsService.Verify(p => p.WriteAsync(
            It.Is<ProblemDetailsContext>(c => c.HttpContext != null
                && c.ProblemDetails.Title != null
                && c.ProblemDetails.Title.Contains("ApiKey")
                && c.ProblemDetails.Detail != null
                && c.ProblemDetails.Detail.Contains("Test Error 1")
                && c.ProblemDetails.Detail.Contains("Test Error 2"))), Times.Once());
    }

    public class TestProblemDetailWriter : IProblemDetailWriter
    {
        private readonly string[] _errors;

        public TestProblemDetailWriter(params string[] errors)
        {
            _errors = errors;
        }

        public IEnumerable<string> GetDetails(HttpContext context, string headerName)
            => _errors;
    }
}