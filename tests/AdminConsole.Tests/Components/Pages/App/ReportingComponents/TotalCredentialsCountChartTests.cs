using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App.ReportingComponents;
using Passwordless.Common.Models.Reporting;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App.ReportingComponents;

public class TotalCredentialsCountChartTests : BunitContext
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IFileVersionProvider> _fileVersionProviderMock = new();

    public TotalCredentialsCountChartTests()
    {
        var items = new Dictionary<object, object> { ["csp-nonce"] = "mocked-nonce-value" };

        // Set up the mock to return a mock HttpContext with the desired Items property
        _httpContextAccessorMock.SetupGet(x => x.HttpContext.Items).Returns(items);

        this.Services.AddSingleton(_httpContextAccessorMock.Object);
        this.Services.AddSingleton(_fileVersionProviderMock.Object);
    }

    [Fact]
    public void Render_Renders_ExpectedId()
    {
        // Arrange
        var data = new List<PeriodicCredentialReportResponse>
        {
            new(new DateOnly(2024, 1, 2), 0, 0),
            new(new DateOnly(2024, 1, 3), 1, 1),
            new(new DateOnly(2024, 1, 4), 1, 2),
            new(new DateOnly(2024, 1, 5), 5, 10)
        }.AsEnumerable();

        var cut = Render<TotalCredentialsCountChart>(parameters => parameters
            .Add(p => p.Data, data));

        // Act
        var actual = cut.Markup;

        // Assert
        Assert.Contains("<div id=\"total-credentials-count-chart\"></div>", actual);
    }
}