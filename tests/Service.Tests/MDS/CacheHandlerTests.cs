using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Service.MDS;
using Xunit.Abstractions;

namespace Passwordless.Service.Tests.MDS;

public class CacheHandlerTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<CacheHandler>> _loggerMock;
    private readonly ITestOutputHelper _output;

    public CacheHandlerTests(ITestOutputHelper output)
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<CacheHandler>>();
        _output = output;
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

    /// <summary>
    /// Verifies that the MDS JWT certificate chain has at least 30 days before expiration.
    /// 
    /// When this test fails, update the MDS JWT file:
    /// 1. Go to https://mds3.fidoalliance.org/
    /// 2. Download the latest MDS BLOB JWT
    /// 3. Replace the contents of src/Api/mds.jwt with the downloaded JWT
    /// 4. Commit and deploy the updated file
    /// </summary>
    [Fact]
    public void MdsJwt_CertificatesShouldBeAtLeast30DaysFromExpiring()
    {
        // arrange
        var jwtPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Api", "mds.jwt"));
        var jwtContent = File.ReadAllText(jwtPath);
        var jwtParts = jwtContent.Trim().Split('.');

        // act
        var headerJson = Encoding.UTF8.GetString(Convert.FromBase64String(AddBase64Padding(jwtParts[0].Replace('-', '+').Replace('_', '/'))));
        using var headerDoc = JsonDocument.Parse(headerJson);
        var x5cProperty = headerDoc.RootElement.GetProperty("x5c");

        var earliestExpiration = DateTime.MaxValue;
        foreach (var certElement in x5cProperty.EnumerateArray())
        {
            var cert = new X509Certificate2(Convert.FromBase64String(certElement.GetString()!));
            if (cert.NotAfter < earliestExpiration)
                earliestExpiration = cert.NotAfter;
            cert.Dispose();
        }

        var daysUntilExpiration = (earliestExpiration - DateTime.UtcNow).TotalDays;
        _output.WriteLine($"MDS certificate expires: {earliestExpiration:yyyy-MM-dd} ({daysUntilExpiration:F0} days)");

        // assert
        Assert.True(daysUntilExpiration > 30,
            $"Certificate expires in {daysUntilExpiration:F0} days. Should be > 30 days.\n" +
            $"To fix: Download latest MDS BLOB from https://mds3.fidoalliance.org/ and replace src/Api/mds.jwt");
    }

    private static string AddBase64Padding(string base64)
    {
        var padding = (4 - base64.Length % 4) % 4;
        return padding > 0 ? base64 + new string('=', padding) : base64;
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