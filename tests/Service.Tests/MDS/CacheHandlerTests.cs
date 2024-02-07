using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Service.MDS;

namespace Passwordless.Service.Tests.MDS;

public class CacheHandlerTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<CacheHandler>> _loggerMock;

    public CacheHandlerTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<CacheHandler>>();
    }

    /// <summary>
    /// We want to verify that the cache handler will return the content from the file '.mds-cache/mds.jwt' when the
    /// request to the MDS server fails.
    /// </summary>
    [Fact]
    public async Task CacheHandler_Returns_ContentFromFile_WhenReceivingUnsuccessfulStatusCode()
    {
        // arrange
        var httpClient = new HttpClient
            (new CacheHandler(new NotFoundResponseHandler(), _configurationMock.Object, _loggerMock.Object))
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        using (var file = File.CreateText(CacheHandler.Path))
        {
            await file.WriteAsync("test");
        }

        // act
        var actual = await httpClient.GetAsync("/api/mds");

        // assert
        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        Assert.Equal("test", await actual.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Handle that the FIDO2 domain name cannot be resolved in offline scenario's.
    /// </summary>
    [Fact]
    public async Task CacheHandler_Returns_ContentFromFile_WhenHttpRequestExceptionWasThrown()
    {
        // arrange
        var httpClient = new HttpClient
            (new CacheHandler(new HttpRequestExceptionResponseHandler(), _configurationMock.Object, _loggerMock.Object))
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        using (var file = File.CreateText(CacheHandler.Path))
        {
            await file.WriteAsync("test");
        }

        // act
        var actual = await httpClient.GetAsync("/api/mds");

        // assert
        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
        Assert.Equal("test", await actual.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Verify we're getting the data from the MDS server when the request is successful.
    /// And that the data is cached locally.
    /// </summary>
    [Fact]
    public async Task CacheHandler_Returns_ContentFromMDS_WhenReceivingSuccessfulStatusCode()
    {
        // arrange
        using (var file = File.CreateText(CacheHandler.Path))
        {
            await file.WriteAsync("test");
        }
        _configurationMock.Setup(x => x["fido2:mds:mode"]).Returns("Online");

        var expectedContent = Guid.NewGuid().ToString();
        var httpClient = new HttpClient
            (new CacheHandler(new OkResponseHandler(expectedContent), _configurationMock.Object, _loggerMock.Object))
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        // act
        var actual = await httpClient.GetAsync("/api/mds");

        // assert

        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);

        var actualContent = await actual.Content.ReadAsStringAsync();
        Assert.NotEqual("test", actualContent);
        Assert.Equal(expectedContent, actualContent);
    }

    private class NotFoundResponseHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        }
    }

    private class OkResponseHandler : HttpMessageHandler
    {
        private readonly string _content;

        public OkResponseHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stringContent = new StringContent(_content, Encoding.UTF8);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = stringContent };
            return Task.FromResult(response);
        }
    }

    private class HttpRequestExceptionResponseHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException();
        }
    }
}