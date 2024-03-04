using AutoFixture;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.AdminConsole.Components.Pages.App;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.Middleware;
using Passwordless.AdminConsole.Models;
using Passwordless.AdminConsole.Services;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Pages.App;

public class LogTests : TestContext
{
    private readonly Mock<ICurrentContext> _currentContextMock = new();
    private readonly Mock<IEventLogService> _eventLogServiceMock = new();
    private readonly Mock<ILogger<Log>> _loggerMock = new();

    private readonly Fixture _fixture = new();

    public LogTests()
    {
        this.Services.AddSingleton(_currentContextMock.Object);
        this.Services.AddSingleton(_eventLogServiceMock.Object);
        this.Services.AddSingleton(_loggerMock.Object);
    }
    
    [Fact]
    public void LogPage_DoesRendersEmptyTable_WhenEventsIsEmpty()
    {
        // Arrange

        var expectedFeatures = _fixture.Build<ApplicationFeatureContext>()
            .With(x => x.EventLoggingIsEnabled, true)
            .With(x => x.EventLoggingRetentionPeriod, 7)
            .Create();
        _currentContextMock.SetupGet(x => x.Features).Returns(expectedFeatures);
        _currentContextMock.SetupGet(x => x.AppId).Returns("myapp");
        var expectedEvents = new ApplicationEventLogResponse("myapp", new List<EventLogEvent>(0), 0);
        _eventLogServiceMock.Setup(x => x.GetEventLogs(
                It.Is<int>(p => p == 1),
                It.Is<int>(p => p == 100)))
            .ReturnsAsync(expectedEvents);
        
        var cut = RenderComponent<Log>(p => p
            .Add(param => param.AppId, "myapp"));

        // Assert
        var actualCell = cut.FindAll("tbody>tr").Single().Children.Single();
        Assert.Equal($"This application has no events from the past 7 days.", actualCell.TextContent);
    }
}