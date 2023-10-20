using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Passwordless.Fido2.MetadataService;
using Passwordless.Fido2.MetadataService.Exceptions;
using Xunit;

namespace Fido2.MetadataService.Tests;

public class MetadataClientTests
{
    private readonly Mock<ILogger<MetadataClient>> _loggerMock = new();
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private MetadataClient _sut;
    
    public MetadataClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri("https://mds.example.com");
        _sut = new MetadataClient(httpClient, _loggerMock.Object);
    }
    
    [Fact]
    public async Task DownloadAsync_Returns_Jwt_WhenRequestSuccessful()
    {
        // arrange
        const string jwt = "eyJhbGciOiJ";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(async (HttpRequestMessage request, CancellationToken token) => {
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StringContent(jwt, Encoding.UTF8);
                return response;
            })
            .Verifiable();;
        
        // act
        var actual = await _sut.DownloadAsync();

        // assert
        Assert.Equal(jwt, actual);
    }
    
    [Fact]
    public async Task DownloadAsync_Throws_Fido2Exception_WhenNonSuccessfulStatusCode()
    {
        // arrange
        const string jwt = "eyJhbGciOiJ";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(async (HttpRequestMessage request, CancellationToken token) => {
                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            })
            .Verifiable();;
        
        // act
        var actual = await Assert.ThrowsAsync<Fido2Exception>(async () => await _sut.DownloadAsync());

        // assert
        Assert.NotNull(actual);
        Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
        Assert.Equal("Failed to obtain latest Fido2 metadata.", actual.Message);
    }
    
    [Fact]
    public async Task DownloadAsync_Throws_Fido2Exception_WhenHttpClientThrowsHttpRequestException()
    {
        // arrange
        const string jwt = "eyJhbGciOiJ";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Throws(new HttpRequestException("Oops", null, HttpStatusCode.BadRequest))
            .Verifiable();;
        
        // act
        var actual = await Assert.ThrowsAsync<Fido2Exception>(async () => await _sut.DownloadAsync());

        // assert
        Assert.NotNull(actual);
        Assert.Equal(HttpStatusCode.BadRequest, actual.StatusCode);
        Assert.Equal("Failed to obtain latest Fido2 metadata.", actual.Message);
    }
}