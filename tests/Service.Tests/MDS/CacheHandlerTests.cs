using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Service.MDS;

namespace Passwordless.Service.Tests.MDS;

public class CacheHandlerTests
{
    private readonly Mock<ILogger<CacheHandler>> _loggerMock;

    public CacheHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CacheHandler>>();
        Directory.CreateDirectory(".mds-cache");
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
            (new CacheHandler(new NotFoundResponseHandler(), _loggerMock.Object))
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        using (var file = File.CreateText(".mds-cache/mds.jwt"))
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
        string? originalContent;
        try
        {
            originalContent = await File.ReadAllTextAsync(CacheHandler.Path);
        }
        catch (FileNotFoundException)
        {
            originalContent = null;
        }

        var httpClient = new HttpClient
            (new CacheHandler(new OkResponseHandler(), _loggerMock.Object))
        {
            BaseAddress = new Uri("http://localhost:5001")
        };

        // act
        var actual = await httpClient.GetAsync("/api/mds");

        // assert

        Assert.Equal(HttpStatusCode.OK, actual.StatusCode);

        var actualContent = await actual.Content.ReadAsStringAsync();
        Assert.NotEqual(originalContent, actualContent);
        Assert.Equal(actualContent, await File.ReadAllTextAsync(CacheHandler.Path));
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
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stringContent = new StringContent(Guid.NewGuid().ToString(), Encoding.UTF8);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = stringContent };
            return Task.FromResult(response);
        }
    }
}